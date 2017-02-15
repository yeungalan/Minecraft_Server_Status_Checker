
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions

Namespace eMZi.Gaming.Minecraft
    ''' <summary>
    ''' Represents retrieved Minecraft Server information
    ''' </summary>
    Public NotInheritable Class MinecraftServerInfo
        ''' <summary>
        ''' Gets the server's MOTD
        ''' </summary>
        Public Property ServerMotd() As String
            Get
                Return m_ServerMotd
            End Get
            Private Set(value As String)
                m_ServerMotd = Value
            End Set
        End Property
        Private m_ServerMotd As String

        ''' <summary>
        ''' Gets the server's MOTD converted into HTML
        ''' </summary>
        Public ReadOnly Property ServerMotdHtml() As String
            Get
                Return Me.MotdHtml()
            End Get
        End Property

        ''' <summary>
        ''' Gets the server's max player count
        ''' </summary>
        Public Property MaxPlayerCount() As Integer
            Get
                Return m_MaxPlayerCount
            End Get
            Private Set(value As Integer)
                m_MaxPlayerCount = Value
            End Set
        End Property
        Private m_MaxPlayerCount As Integer

        ''' <summary>
        ''' Gets the server's current player count
        ''' </summary>
        Public Property CurrentPlayerCount() As Integer
            Get
                Return m_CurrentPlayerCount
            End Get
            Private Set(value As Integer)
                m_CurrentPlayerCount = Value
            End Set
        End Property
        Private m_CurrentPlayerCount As Integer

        ''' <summary>
        ''' Gets the server's Minecraft version
        ''' </summary>
        Public Property MinecraftVersion() As Version
            Get
                Return m_MinecraftVersion
            End Get
            Private Set(value As Version)
                m_MinecraftVersion = Value
            End Set
        End Property
        Private m_MinecraftVersion As Version

        ''' <summary>
        ''' Gets HTML colors associated with specific formatting codes
        ''' </summary>
        Private Shared ReadOnly Property MinecraftColors() As Dictionary(Of Char, String)
            Get
                Return New Dictionary(Of Char, String)() From { _
                    {"0"c, "#000000"}, _
                    {"1"c, "#0000AA"}, _
                    {"2"c, "#00AA00"}, _
                    {"3"c, "#00AAAA"}, _
                    {"4"c, "#AA0000"}, _
                    {"5"c, "#AA00AA"}, _
                    {"6"c, "#FFAA00"}, _
                    {"7"c, "#AAAAAA"}, _
                    {"8"c, "#555555"}, _
                    {"9"c, "#5555FF"}, _
                    {"a"c, "#55FF55"}, _
                    {"b"c, "#55FFFF"}, _
                    {"c"c, "#FF5555"}, _
                    {"d"c, "#FF55FF"}, _
                    {"e"c, "#FFFF55"}, _
                    {"f"c, "#FFFFFF"} _
                }
            End Get
        End Property

        ''' <summary>
        ''' Gets HTML styles associated with specific formatting codes
        ''' </summary>
        Private Shared ReadOnly Property MinecraftStyles() As Dictionary(Of Char, String)
            Get
                Return New Dictionary(Of Char, String)() From { _
                    {"k"c, "none;font-weight:normal;font-style:normal"}, _
                    {"m"c, "line-through;font-weight:normal;font-style:normal"}, _
                    {"l"c, "none;font-weight:900;font-style:normal"}, _
                    {"n"c, "underline;font-weight:normal;font-style:normal;"}, _
                    {"o"c, "none;font-weight:normal;font-style:italic;"}, _
                    {"r"c, "none;font-weight:normal;font-style:normal;color:#FFFFFF;"} _
                }
            End Get
        End Property

        ''' <summary>
        ''' Creates a new instance of <see cref="MinecraftServerInfo"/> with specified values
        ''' </summary>
        ''' <param name="motd">Server's MOTD</param>
        ''' <param name="maxplayers">Server's max player count</param>
        ''' <param name="playercount">Server's current player count</param>
        ''' <param name="version">Server's Minecraft version</param>
        Private Sub New(motd As String, maxplayers As Integer, playercount As Integer, mcversion As Version)
            Me.ServerMotd = motd
            Me.MaxPlayerCount = maxplayers
            Me.CurrentPlayerCount = playercount
            Me.MinecraftVersion = mcversion
        End Sub

        ''' <summary>
        ''' Gets the server's MOTD formatted as HTML
        ''' </summary>
        ''' <returns>HTML-formatted MOTD</returns>
        Private Function MotdHtml() As String
            Dim regex As New Regex("§([k-oK-O])(.*?)(§[0-9a-fA-Fk-oK-OrR]|$)")
            Dim s As String = Me.ServerMotd
            While regex.IsMatch(s)
                s = regex.Replace(s, Function(m)
                                                     Dim ast As String = "text-decoration:" + MinecraftStyles(m.Groups(1).Value(0))
                                                     Dim html As String = (Convert.ToString("<span style=""") & ast) + """>" + m.Groups(2).Value + "</span>" + m.Groups(3).Value
                                                     Return html

                                                 End Function)
            End While
            regex = New Regex("§([0-9a-fA-F])(.*?)(§[0-9a-fA-FrR]|$)")
            While regex.IsMatch(s)
                s = regex.Replace(s, Function(m)
                                                     Dim ast As String = "color:" + MinecraftColors(m.Groups(1).Value(0))
                                                     Dim html As String = (Convert.ToString("<span style=""") & ast) + """>" + m.Groups(2).Value + "</span>" + m.Groups(3).Value
                                                     Return html

                                                 End Function)
            End While
            Return s
        End Function

        ''' <summary>
        ''' Gets the information from specified server
        ''' </summary>
        ''' <param name="endpoint">IP and Port of the server to get information from</param>
        ''' <returns>A <see cref="MinecraftServerInformation"/> instance with retrieved data</returns>
        ''' <exception cref="System.Exception">Upon failure, throws exception with descriptive information and InnerException with details</exception>
        Public Shared Function GetServerInformation(endpoint As IPEndPoint) As MinecraftServerInfo
            If endpoint Is Nothing Then
                Throw New ArgumentNullException("endpoint")
            End If
            Try
                Dim packetdat As String() = Nothing
                Using client As New TcpClient()
                    client.Connect(endpoint)
                    Using ns As NetworkStream = client.GetStream()
                        ns.Write(New Byte() {&HFE, &H1}, 0, 2)
                        Dim buff As Byte() = New Byte(2047) {}
                        Dim br As Integer = ns.Read(buff, 0, buff.Length)
                        If buff(0) <> &HFF Then
                            Throw New InvalidDataException("Received invalid packet")
                        End If
                        Dim packet As String = Encoding.BigEndianUnicode.GetString(buff, 3, br - 3)
                        If Not packet.StartsWith("§") Then
                            Throw New InvalidDataException("Received invalid data")
                        End If
                        packetdat = packet.Split(ControlChars.NullChar)
                        ns.Close()
                    End Using
                    client.Close()
                End Using
                Return New MinecraftServerInfo(packetdat(3), Integer.Parse(packetdat(5)), Integer.Parse(packetdat(4)), Version.Parse(packetdat(2)))
            Catch ex As SocketException
                Throw New Exception("There was a connection problem, look into InnerException for details", ex)
            Catch ex As InvalidDataException
                Throw New Exception("The data received was invalid, look into InnerException for details", ex)
            Catch ex As Exception
                Throw New Exception("There was a problem, look into InnerException for details", ex)
            End Try
        End Function

        ''' <summary>
        ''' Gets the information from specified server
        ''' </summary>
        ''' <param name="ip">IP of the server to get info from</param>
        ''' <param name="port">Port of the server to get info from</param>
        ''' <returns>A <see cref="MinecraftServerInformation"/> instance with retrieved data</returns>
        ''' <exception cref="System.Exception">Upon failure, throws exception with descriptive information and InnerException with details</exception>
        Public Shared Function GetServerInformation(ip As IPAddress, port As Integer) As MinecraftServerInfo
            Return GetServerInformation(New IPEndPoint(ip, port))
        End Function
    End Class
End Namespace

'=======================================================
'Service provided by Telerik (www.telerik.com)
'Conversion powered by NRefactory.
'Twitter: @telerik
'Facebook: facebook.com/telerik
'=======================================================
