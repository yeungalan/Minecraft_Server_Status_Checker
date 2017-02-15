Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Diagnostics
Imports System.IO
Imports System.Text.RegularExpressions

Module Module1

    Sub Main()



        Dim soc As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        soc.Bind(New IPEndPoint(IPAddress.Any, 8585))
        soc.Listen(10)
        Console.WriteLine("Web Server Started ...")

        While True
            Try



                Dim client As Socket = soc.Accept
                Dim sb As New System.Text.StringBuilder
                Dim html As String = File.ReadAllText("index.html")
                Dim htmlHeader As String = _
                "HTTP/1.0 200 OK" & ControlChars.CrLf & _
                "Server: WebServer 1.0" & ControlChars.CrLf & _
                "Content-Length: " & html.Length & ControlChars.CrLf & _
                "Content-Type: text/html" & _
                ControlChars.CrLf & ControlChars.CrLf

                Dim headerByte() As Byte = Encoding.ASCII.GetBytes(htmlHeader)
                client.Send(headerByte, headerByte.Length, SocketFlags.None)

                client.Receive(headerByte, headerByte.Length, SocketFlags.None)
                'Console.WriteLine(Encoding.ASCII.GetString(headerByte).Split(" ")(1))

                Dim txt As String = Encoding.ASCII.GetString(headerByte).Split(" ")(1)
                
                Dim ip As String = Nothing
                Dim iph As IPHostEntry = Dns.GetHostEntry(txt.Replace("/", ""))
                Console.WriteLine(txt.Replace("/", ""))
                For Each ip1 As IPAddress In iph.AddressList
                    ip = ip1.ToString()
                    ' TODO: might not be correct. Was : Exit For
                    Exit For
                Next
                Dim a As eMZi.Gaming.Minecraft.MinecraftServerInfo = Nothing
                Dim c As IPAddress = Nothing
                Dim Err As Boolean = False
                c = IPAddress.Parse(ip)
                Dim b As New IPEndPoint(c, 25565)

                Try
                    a = eMZi.Gaming.Minecraft.MinecraftServerInfo.GetServerInformation(b)
                Catch ex As Exception
                    html = html.Replace("{0}", "").Replace("{1}", "").Replace("{2}", "").Replace("{3}", "")
                    Dim htmlByte1() As Byte = Encoding.ASCII.GetBytes(html)

                    client.Send(htmlByte1, 0, htmlByte1.Length, SocketFlags.None)
                    Err = True
                End Try

                If Err = False Then
                    a = eMZi.Gaming.Minecraft.MinecraftServerInfo.GetServerInformation(b)
                    html = html.Replace("{0}", txt.Replace("/", "")).Replace("{1}", a.ServerMotd).Replace("{2}", a.CurrentPlayerCount & "/" & a.MaxPlayerCount).Replace("{3}", a.MinecraftVersion.ToString)
                    Dim htmlByte() As Byte = Encoding.ASCII.GetBytes(html)

                    client.Send(htmlByte, 0, htmlByte.Length, SocketFlags.None)

                End If





            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End While





    End Sub




End Module