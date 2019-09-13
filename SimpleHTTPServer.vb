Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Net.Sockets
Imports System.Net
Imports System.IO
Imports System.Threading
Imports System.Diagnostics

Class SimpleHTTPServer
    Private ReadOnly _indexFiles As String() = {"index.html", "index.htm", "default.html", "default.htm"}
    Private Shared _mimeTypeMappings As IDictionary(Of String, String) = New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase) From {
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"}
    }
    Private _serverThread As Thread
    Private _rootDirectory As String
    Private _listener As HttpListener
    Private _port As Integer

    Public Property Port As Integer
        Get
            Return _port
        End Get
        Private Set(ByVal value As Integer)
        End Set
    End Property

    Public Sub New(ByVal path As String, ByVal port As Integer)
        Me.Initialize(path, port)
    End Sub

    Public Sub New(ByVal path As String)
        Dim l As TcpListener = New TcpListener(IPAddress.Loopback, 0)
        l.Start()
        Dim port As Integer = (CType(l.LocalEndpoint, IPEndPoint)).Port
        l.[Stop]()
        Me.Initialize(path, port)
    End Sub

    Public Sub [Stop]()
        _serverThread.Abort()
        _listener.[Stop]()
    End Sub

    Private Sub Listen()
        _listener = New HttpListener()
        _listener.Prefixes.Add("http://+:" & _port.ToString() & "/")
        _listener.Start()

        While True

            Try
                Dim context As HttpListenerContext = _listener.GetContext()
                Process(context)
            Catch ex As Exception
            End Try
        End While
    End Sub

    Private Sub Process(ByVal context As HttpListenerContext)
        Dim filename As String = context.Request.Url.AbsolutePath
        Console.WriteLine(filename)
        filename = filename.Substring(1)

        If String.IsNullOrEmpty(filename) Then

            For Each indexFile As String In _indexFiles

                If File.Exists(Path.Combine(_rootDirectory, indexFile)) Then
                    filename = indexFile
                    Exit For
                End If
            Next
        End If

        filename = Path.Combine(_rootDirectory, filename)

        If File.Exists(filename) Then

            Try
                Dim input As Stream = New FileStream(filename, FileMode.Open)
                Dim mime As String
                context.Response.ContentType = If(_mimeTypeMappings.TryGetValue(Path.GetExtension(filename), mime), mime, "application/octet-stream")
                context.Response.ContentLength64 = input.Length
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"))
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"))
                Dim buffer As Byte() = New Byte(16383) {}
                Dim nbytes As Integer

                While (CSharpImpl.__Assign(nbytes, input.Read(buffer, 0, buffer.Length))) > 0
                    context.Response.OutputStream.Write(buffer, 0, nbytes)
                End While

                input.Close()
                context.Response.StatusCode = CInt(HttpStatusCode.OK)
                context.Response.OutputStream.Flush()
            Catch ex As Exception
                context.Response.StatusCode = CInt(HttpStatusCode.InternalServerError)
            End Try
        Else
            context.Response.StatusCode = CInt(HttpStatusCode.NotFound)
        End If

        context.Response.OutputStream.Close()
    End Sub

    Private Sub Initialize(ByVal path As String, ByVal port As Integer)
        Me._rootDirectory = path
        Me._port = port
        _serverThread = New Thread(AddressOf Me.Listen)
        _serverThread.Start()
    End Sub

    Private Class CSharpImpl
        <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
        Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Class
