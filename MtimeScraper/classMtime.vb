Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Net
Imports System.IO

Public Class Mtime

#Region "> ======== ASSEMBLY INFO ========"
    Public Shared ReadOnly Property libraryName() As String
        Get
            Return Title & " " & Version.ToString(".00")
        End Get
    End Property

    Public Shared ReadOnly Property Title() As String
        Get
            Return CType(Reflection.AssemblyTitleAttribute.GetCustomAttribute( _
            Reflection.Assembly.GetExecutingAssembly, _
            GetType(Reflection.AssemblyTitleAttribute)), Reflection.AssemblyTitleAttribute).Title
        End Get
    End Property

    Public Shared ReadOnly Property Version() As Decimal
        Get
            With Reflection.Assembly.GetExecutingAssembly.GetName.Version
                Return CDec(.Major & "." & .Minor)
            End With
        End Get
    End Property
#End Region

#Region "> ======== CONSTRUCTOR ========"
    Const userAgent As String = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:26.0) Gecko/20100101 Firefox/26.0"
    Const accePt As String = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
    Const acceptCharset As String = "ISO-8859-1,utf-8;q=0.7,*;q=0.7"
    Const acceptLanguage As String = "en-us,en;q=0.5"
    Private Shared random As New Random
    Private mSource As String
    Private classSc() As String

    Sub New(ByVal mSource As String, ByVal classSc() As String)
        Me.mSource = charRef.tightenWidth(Regex.Replace(mSource, "&nbsp;", " "))
        Me.classSc = classSc
    End Sub

    Sub New(ByVal movNo As String, ByVal pageNo As String, ByVal classSc() As String, ByVal cookies As String, Optional ByVal timeOut As Integer = 5000)
        Dim mLink As String
        Select Case pageNo
            'Scramble
            Case 0 : mLink = "http://service.mtime.com/css/scramble.css?" & Now.ToString("yyyy%M%d%h%m%s") & random.Next(1000, 9999)
                'Main
            Case 1 : mLink = "http://movie.mtime.com/" & movNo & "/"
                'Credit
            Case 2 : mLink = "http://movie.mtime.com/" & movNo & "/fullcredits.html"
                'Plot
            Case 3 : mLink = "http://service.mtime.com/database/ShowPlotService.m?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Community.Controls.CommunityPages." & _
            "ShowPlotService&Ajax_CallBackMethod=LoadData&Ajax_RequestUrl=http%3A%2F%2Fmovie.mtime.com%2F" & movNo & _
            "%2Fplots.html&t=" & Now.ToString("yyyyMMddHHmmss") & "&Ajax_CallBackArgument0=1&Ajax_CallBackArgument1=" & movNo
                'Extras
            Case 4 : mLink = "http://service.mtime.com/database/showBehindTheSceneService.m?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Community.Controls.CommunityPages." & _
            "ShowBehindTheSceneService&Ajax_CallBackMethod=LoadData&Ajax_RequestUrl=http%3A%2F%2Fmovie.mtime.com%2F" & movNo & _
            "%2Fbehind_the_scene.html&t=" & Now.ToString("yyyyMMddHHmmss") & "&Ajax_CallBackArgument0=1&Ajax_CallBackArgument1=" & movNo
                'Follow movNo
            Case Else : mLink = movNo
        End Select

        Dim requestScrape As HttpWebRequest = WebRequest.Create(mLink)
        Dim responseScrape As HttpWebResponse
        Dim sReader As StreamReader
        requestScrape.KeepAlive = False
        requestScrape.Timeout = timeOut
        requestScrape.ReadWriteTimeout = timeOut * 1.5
        requestScrape.Accept = accePt
        requestScrape.Headers.Add("Accept-Charset", acceptCharset)
        requestScrape.Headers.Add("Accept-Language", acceptLanguage)
        requestScrape.UserAgent = userAgent
        requestScrape.AutomaticDecompression = DecompressionMethods.GZip
        If cookies <> String.Empty Then
            requestScrape.CookieContainer = New CookieContainer()
            Select Case pageNo
                Case 1, 2 : requestScrape.CookieContainer.Add(New Uri("http://movie.mtime.com/"), New Cookie("_mi_", cookies))
                Case 0, 3, 4 : requestScrape.CookieContainer.Add(New Uri("http://service.mtime.com/"), New Cookie("_mi_", cookies))
                    requestScrape.CookieContainer.Add(New Uri("http://service.mtime.com/"), New Cookie("NSC_192.168.0.103", "8efb362b3660"))
            End Select
        End If
        responseScrape = requestScrape.GetResponse
        sReader = New StreamReader(responseScrape.GetResponseStream)
        mSource = charRef.tightenWidth(Regex.Replace(sReader.ReadToEnd, "&nbsp;", " "))
        sReader.Close()
        responseScrape.Close()

        Me.classSc = classSc
    End Sub
#End Region

#Region "> ======== SHARED FUNCTION ========"
    Public Shared Function scScraper(ByVal cookies As String, Optional ByVal timeOut As Integer = 5000) As String()
        'Used to Load & Refine Mtime Scrambles Rule
        Dim Match As Match
        Dim nCtr As Integer = 0
        Dim classSc() As String = {""}
        Dim mSource As String = String.Empty
        'Corresponding javascript counterpart: DateDiff(DateInterval.Second, DateTime.Parse("#1/1/1970#"), DateTime.UtcNow) * 1000 + random.Next(0, 999)
        Dim requestScrape As HttpWebRequest = WebRequest.Create( _
        "http://service.mtime.com/css/scramble.css?" & Now.ToString("yyyy%M%d%h%m%s") & _
        random.Next(1000, 9999))
        Dim responseScrape As HttpWebResponse
        Dim sReader As StreamReader
        requestScrape.KeepAlive = False
        requestScrape.Timeout = timeOut
        requestScrape.ReadWriteTimeout = timeOut * 2
        requestScrape.Accept = accePt
        requestScrape.Headers.Add("Accept-Charset", acceptCharset)
        requestScrape.Headers.Add("Accept-Language", acceptLanguage)
        requestScrape.UserAgent = userAgent
        requestScrape.CookieContainer = New CookieContainer()
        requestScrape.CookieContainer.Add(New Uri("http://service.mtime.com/"), New Cookie("_mi_", cookies))
        requestScrape.CookieContainer.Add(New Uri("http://service.mtime.com/"), New Cookie("NSC_192.168.0.103", "8efb362b3660"))
        responseScrape = requestScrape.GetResponse
        sReader = New StreamReader(responseScrape.GetResponseStream)
        mSource = sReader.ReadToEnd()
        'Throw New Exception("There was an error retrieving the Web page you requested. Please check the Url and your connection to the Internet, and try again.")
        sReader.Close()
        responseScrape.Close()

        Match = Regex.Match(mSource, "(.(?<class>m_s\d+)\{[\w:;]+\}\s*)+")
        If Match.Groups("class").Success Then
            For Each c As Capture In Match.Groups("class").Captures
                '.m_s0{display:none;}.m_s2{display:none;}
                classSc(nCtr) = c.Value
                If nCtr >= classSc.Length - 1 Then
                    ReDim Preserve classSc(nCtr + 10)
                End If
                nCtr += 1
            Next
            ReDim Preserve classSc(nCtr - 1)
        End If
        Return classSc
    End Function

    Public Shared Function loginChecker(ByVal cookies As String, Optional ByVal timeOut As Integer = 5000) As String
        'Used to Identify Login State using saved cookies

        'revision 1.36 : 
        'URL   http://www.mtime.com/Service/User.m?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Community.Controls.CommunityPages.UserService&Ajax_CallBackMethod=GetSignInHTML&Ajax_RequestUrl=http%3A%2F%2Fwww.mtime.com%2F&t=20107113855695&Ajax_CallBackArgument0=1
        'TRUE  var getSignInHTMLResult = { value:{"isLogin":true,"html":"<div class=\"logined\"
        'FALSE var getSignInHTMLResult = { value:{"signInHtml":"<div class=\"login-ready\">
        'MATCH LOGIN    If Regex.IsMatch(mSource, """value"":{""userId""") Then
        'REPLY  Return "Mtime Nick """ & Regex.Match(mSource, """nickName"":""(?<nick>[^""]+)").Groups("nick").Value & """"
        'Dynamic URL
        'Dim requestScrape As HttpWebRequest = WebRequest.Create( _
        '"http://service.mtime.com/Service/User.m?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Community.Controls.CommunityPages." & _
        '"UserService&Ajax_CallBackMethod=GetSignInHTML&Ajax_RequestUrl=http%3A%2F%2Fwww.mtime.com%2F&t=" & _
        'Now.ToString("yyyyMMddHHmmss") & random.Next(1000, 9999) & "&Ajax_CallBackArgument0=1")
        'MATCH LOGIN    If Regex.IsMatch(mSource, """value"":{""isLogin""") Then
        'REPLY  Return "Mtime User """ & Regex.Match(mSource, "<a href=\\""http://i?\.?mtime\.com/\d+/\\?"">(?<id>[^<]+)</a>").Groups("id").Value & """"

        'revision 1.37 : 
        'URL    http://service.mtime.com/Service/Manager.msi?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Service.Pages.ManagerService&Ajax_CallBackMethod=GetUserInfo2&Ajax_RequestUrl=http%3A%2F%2Fmy.mtime.com%2Fprofile%2Fbasic%2F&t=20111216339770778
        'TRUE   var result_20111216339770778 = { "value":{"userId":1234567,"nickName":"User1234567",
        'FALSE  var result_20111216339770778 = { "value":{"forgetPasswordUrl":"http://my.mtime.com/member/forget_password/",
        'MATCH LOGIN    If Regex.IsMatch(mSource, """value"":{""userId""") Then
        'REPLY  Return "Mtime Nick """ & Regex.Match(mSource, """nickName"":""(?<nick>[^""]+)").Groups("nick").Value & """"
        'Dynamic URL
        'Dim requestScrape As HttpWebRequest = WebRequest.Create( _
        '"http://service.mtime.com/Service/Manager.msi?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Service.Pages." & _
        '"ManagerService&Ajax_CallBackMethod=GetUserInfo2&Ajax_RequestUrl=http%3A%2F%2Fmy.mtime.com%2Fprofile%2Fbasic%2F&t=" & _
        'Now.ToString("yyyyMMddHHmmss") & random.Next(1000, 9999))

        Dim mSource As String = String.Empty
        Try
            Dim requestScrape As HttpWebRequest = WebRequest.Create( _
            "http://service.mtime.com/Service/Manager.msi?Ajax_CallBack=true&Ajax_CallBackType=Mtime.Service.Pages." & _
            "ManagerService&Ajax_CallBackMethod=GetUserInfo2&Ajax_RequestUrl=http%3A%2F%2Fmy.mtime.com%2Fprofile%2Fbasic%2F&t=" & _
            Now.ToString("yyyyMMddHHmmss") & random.Next(1000, 9999))
            Dim responseScrape As HttpWebResponse
            Dim sReader As StreamReader
            requestScrape.KeepAlive = False
            requestScrape.Timeout = timeOut
            requestScrape.ReadWriteTimeout = timeOut * 1.5
            requestScrape.Accept = accePt
            requestScrape.Headers.Add("Accept-Charset", acceptCharset)
            requestScrape.Headers.Add("Accept-Language", acceptLanguage)
            requestScrape.UserAgent = userAgent
            requestScrape.AutomaticDecompression = DecompressionMethods.GZip
            requestScrape.CookieContainer = New CookieContainer()
            requestScrape.CookieContainer.Add(New Uri("http://service.mtime.com/"), New Cookie("_mi_", cookies))
            responseScrape = requestScrape.GetResponse
            sReader = New StreamReader(responseScrape.GetResponseStream)
            mSource = sReader.ReadToEnd()
            sReader.Close()
            responseScrape.Close()
        Catch ex As Exception
        End Try

        If Regex.IsMatch(mSource, """value"":{""userId""") Then
            Return "Mtime Nick """ & Regex.Match(mSource, """nickName"":""(?<nick>[^""]+)").Groups("nick").Value & """"
        Else
            Return "Mtime User Not Login."
        End If
    End Function

    Public Shared Function loginSpider(ByVal userName As String, ByVal passWord As String, Optional ByVal timeOut As Integer = 5000) As String
        'Corresponding javascript counterpart:
        ' getToken:function(){var D=new Date(),C=[];C.push(D.getFullYear());C.push(D.getMonth()+1);C.push(D.getDate());
        ' C.push(D.getHours());C.push(D.getMinutes());C.push(D.getSeconds());C.push(parseInt(Math.random()*10000,10));return C.join("");}

        'javascript function as below: C.push(parseInt(Math.random()*10000,10));
        'return a random integer between 0 and 9999						    Math.random()*10000
        'parses a string and returns an integer parseInt(string, radix) 	parseInt(string,10)
        Try
            Dim cookies As String = String.Empty
            Dim requestScrape As HttpWebRequest = WebRequest.Create( _
            "http://my.mtime.com/member/signin/?t=" & Now.ToString("yyyyMMddHHmmss") & random.Next(1000, 9999))

            Dim responseScrape As HttpWebResponse
            Dim requestStream As Stream
            Dim postData As String = "email=" & userName & "&password=" & passWord
            Dim byteStream As Byte() = System.Text.Encoding.GetEncoding(0).GetBytes(postData)
            requestScrape.KeepAlive = False
            requestScrape.Timeout = timeOut
            requestScrape.ReadWriteTimeout = timeOut * 1.5
            requestScrape.Method = "POST"
            requestScrape.ContentType = "application/x-www-form-urlencoded"
            requestScrape.ContentLength = postData.Length
            requestScrape.Referer = "http://my.mtime.com/member/signin/"
            requestScrape.Accept = accePt
            requestScrape.Headers.Add("Accept-Charset", acceptCharset)
            requestScrape.Headers.Add("Accept-Language", acceptLanguage)
            requestScrape.UserAgent = userAgent
            requestScrape.AutomaticDecompression = DecompressionMethods.GZip

            requestScrape.CookieContainer = New CookieContainer()
            requestStream = requestScrape.GetRequestStream()
            requestStream.Write(byteStream, 0, byteStream.Length)
            responseScrape = requestScrape.GetResponse
            requestStream.Close()
            responseScrape.Close()

            'LOGIN FAIL     http://www.mtime.com/member/signin/?t='getToken()'
            'LOGIN SUCCESS  http://i.mtime.com/1234567/
            If requestScrape.CookieContainer.GetCookies(requestScrape.RequestUri).Count >= 1 And _
            Not Regex.IsMatch(responseScrape.ResponseUri.ToString, "/member/signin/", RegexOptions.IgnoreCase) Then
                For Each cook As Cookie In requestScrape.CookieContainer.GetCookies(requestScrape.RequestUri)
                    cookies = cook.Value
                Next
                If cookies <> String.Empty Then : Return cookies : End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function linkValidator(ByVal url As String) As Boolean
        Return Regex.IsMatch(url.Trim, "\A(http://\w+.mtime.\w+/)?\d{5,6}/?")
    End Function

    Public Shared Function getMovNo(ByVal url As String) As String
        Return Regex.Match(url.Trim, "\A(http://\w+.mtime.\w+/)?(?<mov>\d{5,6})/?").Groups("mov").Value
    End Function

    Private Function scCleaner(ByVal m As Match) As String
        For Each sc As String In classSc
            If String.Equals(sc, m.Groups("class").Captures.Item(0).Value) Then
                Return String.Empty
            End If
        Next
        Return m.Groups("word").Captures.Item(0).Value
    End Function

    Private Function alignReview(ByVal m As Match) As String
        'revision 1.02 : 
        'If m.Groups("classA").Value <> m.Groups("classB").Value Then
        '    Return m.Groups("review").Value.Trim & m.Groups("author").Value.Trim
        'Else : Return m.Value : End If
        Return Regex.Replace(m.Groups("content").Value, "(<((?<!>).)+\s*){2}", String.Empty)
    End Function

    Public Function checkAvailability() As Boolean
        If Regex.Match(mSource, """paragraghs"":""").Success Then : Return True
        Else : Return False : End If
    End Function

    Public Function checkDirector() As Boolean
        If Regex.IsMatch(mSource, "导演 Director:\s*</h3>") Then : Return True
        Else : Return False : End If
    End Function

    Public Function checkCast() As Boolean
        If Regex.IsMatch(mSource, "演员 Actor:\s*</h3>") Then : Return True
        Else : Return False : End If
    End Function
#End Region

#Region "> ======== MAIN SECTION ========"
    Public ReadOnly Property getSource() As String
        Get
            Return mSource
        End Get
    End Property

    Public Function getTitle() As String
        ''Title
        'revision 1.36 :
        'Iron Man 2, 83329, Case with chinese & english title
        '<h1 class="movie_film_nav normal pl9 pr15">	<a href="/83329/" class="px28 bold hei c_000"><span property="v:itemreviewed">钢铁侠2</span></a><a href="/83329/" class="ml9 px24">Iron Man 2</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/2010/" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Aftershock, 99400
        '<h1 class="movie_film_nav normal pl9 pr15">	<a href="/99400/" class="px28 bold hei c_000"><span property="v:itemreviewed">唐山大地震</span></a><a href="/99400/" class="ml9 px24">Aftershock</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/2010/" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Ganja & Hess, 70374, Case with Only English Title, IGNORE posting on GUI
        '<h1 class="movie_film_nav normal pl9 pr15">	<a href="/70374/" class="ml9 px24">Ganja & Hess</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/1973/" >1973</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Phantom Thunderbolt, 71722
        '<h1 class="movie_film_nav normal pl9 pr15">	<a href="/71722/" class="ml9 px24">Phantom Thunderbolt</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/1933/" >1933</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>

        'revision 1.37 : 
        'Iron Man 2, 83329, Case with chinese & english title
        '<h1 class="movie_film_nav normal pl9 pr15">	<span class="px28 bold hei c_000" property="v:itemreviewed">钢铁侠2</span><span class="ml9 px24">Iron Man 2</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=2010" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Aftershock, 99400
        '<h1 class="movie_film_nav normal pl9 pr15">	<span class="px28 bold hei c_000" property="v:itemreviewed">唐山大地震</span><span class="ml9 px24">Aftershock</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=2010" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Ganja & Hess, 70374, Case with Only English Title, IGNORE posting on GUI
        '<h1 class="movie_film_nav normal pl9 pr15">	<span class="ml9 px24">Ganja & Hess</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=1973" >1973</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Phantom Thunderbolt, 71722
        '<h1 class="movie_film_nav normal pl9 pr15">	<span class="ml9 px24">Phantom Thunderbolt</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=1933" >1933</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>

        'revision 1.32 : Return charRef.cleanRef(Regex.Match(mSource, "h1 class(\s*<?((?<!>).)+)+\s*((?!<a href).)+\s*((?!>).)+>(?<title>((?!<).)+)", RegexOptions.IgnoreCase).Groups("title").Value.Trim)
        'revision 1.36 : Return charRef.cleanRef(Regex.Replace(Match.Groups("title").Captures(Match.Groups("title").Captures.Count - 1).Value, "(<((?<!>).)+)+", String.Empty).Trim)

        'revision 1.36 : Regex.Match(mSource, "h1 class((?!<a href|<a class|</h1).)+(?<title>\s*<a href((?<!</a>|<a class|</h1>).)+)+", RegexOptions.IgnoreCase)
        'revision 1.37 : Regex.Match(mSource, "h1 class((?!<a href|<a class|</h1).)+(?<title>\s*<span((?!</a>|\(<a class|</h1>).)+)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "h1 class((?!<span|<a href|<a class|</h1).)+(?<title>\s*<span((?<!</span>|</a>|\(<a class|</h1>).)+)+", RegexOptions.IgnoreCase)
        If Match.Groups("title").Success Then
            Return charRef.cleanRef( _
            Regex.Replace(Match.Groups("title").Captures(Match.Groups("title").Captures.Count - 1).Value, _
                          "(<((?<!>).)+)+", String.Empty).Trim)
        Else : Return String.Empty : End If
    End Function

    Public Function getTitleCn() As String
        ''Chinese Title (China)
        'Echoes Of The Rainbow, 102279, Contained official blog link
        '<h1 class="movie_film_nav normal pl9 pr15">	<a href="/102279/" class="px28 bold hei c_000">岁月神偷</a>
        '<a href="/102279/" class="ml9 px24">Echoes Of The Rainbow</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/section/year/2010/" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>

        '.tester
        'header <h1 class="movie_film_nav normal pl9 pr15"><span class="fr mt15 pt12">
        'optional <a href="http://www.mtime.com/feature/suiyue.html" target="_blank" class="btn_blue fl mr15">官方博客</a><a href="/feature/suiyue.html" target="_blank" class="btn_blue fl">相关专题</a>
        'header </span>
        'optional <a href="/movie/102279/" class="px28 bold hei c_000">岁月神偷</a>
        'middle <a href="/movie/102279/" class="ml9 px24">Echoes Of The Rainbow</a>
        'footer ' <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/movie/section/year/2010/" >2010</a>)
        'footer ' <span class="ml9 px14">
        'footer </span></em></h1>
        '.potion
        'header <h1(\s*[<\w""'=_ ]+>){2}
        'optional (<[\w\-\%/""':.=+_ ]+>\w+</\w+>)*
        'header </\w+>
        'optional <[\w\-\%/""':.=+_ ]+>(?<zhtitle>[]+)</\w+>
        'middle <[\w\-\%/""':.=+_ ]+>[]+</\w+>
        'footer  \<[\w""'=_ ]+>\s*\(<[\w\-\%/""':.=+_ ]+>\d+</a>\)
        'footer <[\w""'=_ ]+>
        'footer </span></em></h1>
        'revision 1.02 : Match = Regex.Match(tightenWidth(mSource), "<h1 [\w\+\-\|/\\""'&=_ ]+>(\s*<span [\w\+\-\|/\\""'&=_ ]+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+</\w+>)*\s*</span>)?(\s*<a [\w\+\-\.\?\|/\\""'#%&:;=_ ]+>(?<zhtitle>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)</a>)?\s*<a [\w\+\-\.\?\|/\\""'#%&:;=_ ]+>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+</a>", RegexOptions.IgnoreCase)
        'revision 1.36 : Regex.Match(mSource, "h1 class((?!<a href|<a class|</h1).)+(?<title>\s*<a href((?<!</a>|<a class|</h1>).)+)+", RegexOptions.IgnoreCase)
        'revision 1.37 : Regex.Match(mSource, "h1 class((?!<span|<a href|<a class|</h1).)+(?<title>\s*<span((?<!</span>|</a>|\(<a class|</h1>).)+)+", RegexOptions.IgnoreCase)

        Dim Match As Match = Regex.Match(mSource, "h1 class((?!<span|<a href|<a class|</h1).)+(?<title>\s*<span((?<!</span>|</a>|\(<a class|</h1>).)+)+", RegexOptions.IgnoreCase)
        If Match.Groups("title").Success AndAlso Match.Groups("title").Captures.Count > 1 Then
            Dim strTitleCn As String = Regex.Replace(Match.Groups("title").Captures(0).Value, "(<((?<!>).)+)+", String.Empty).Trim
            Dim mMore As Match = Regex.Match(mSource, "更多中文名:(\s*<((?<!>).)+)+(?<title>\s*<((?!</li>|</ul|</div|<h).)+</(li|ul)>)+", RegexOptions.IgnoreCase)
            If mMore.Success Then
                For Each c As Capture In mMore.Groups("title").Captures
                    If Not c.Value.Contains("香港") AndAlso Not c.Value.Contains("台湾") Then
                        strTitleCn &= "/" & Regex.Replace(c.Value, "(<((?<!>).)+)+", String.Empty).Trim
                    End If
                Next
            End If
            Return charRef.cleanRef(strTitleCn)
        Else : Return String.Empty : End If

        'revision 1.32 : 
        'Dim Match As Match = Regex.Match(mSource, "h1 class(\s*<?((?<!>).)+)+\s*(?<title>((?!<).)+)", RegexOptions.IgnoreCase)
        'Dim strTitleCn As String = String.Empty
        'If Match.Success Then
        '    strTitleCn = Match.Groups("title").Value.TrimEnd
        '    Dim mMore As Match = Regex.Match(mSource, "更多中文名:(\s*<((?<!>).)+)+(?<title>\s*<((?!</li>|</ul|</div|<h).)+</(li|ul)>)+", RegexOptions.IgnoreCase)
        '    If mMore.Success Then
        '        For Each c As Capture In mMore.Groups("title").Captures
        '            If Not c.Value.Contains("香港") AndAlso Not c.Value.Contains("台湾") Then
        '                strTitleCn &= "/" & Regex.Replace(c.Value, "(<((?<!>).)+)+", String.Empty).Trim
        '            End If
        '        Next
        '    End If
        'End If
        'Return charRef.cleanRef(strTitleCn)
    End Function

    Public Function getTitleHk() As String
        ''Chinese Title (Hong Kong)
        '钢铁人2
        '<h3 class="mt3">更多中文名:</h3>	<ul>		<li class="mt6">钢铁人2<span class="c_a5">.....台湾译名</span></li>		<li class="mt6">铁甲奇侠2<span class="c_a5">.....香港译名</span></li>		</ul>
        '岁月神偷
        '<h3 class="mt3">更多中文名:</h3>	<ul>		<li class="mt6">1969太空漫游<span class="c_a5"></span></li>		</ul>
        '104790
        '<h3 class="mt3">更多中文名:</h3>	<ul>		<li class="mt6">腐女攻略<span class="c_a5">.....香港译名</span></li>		</ul>		</div>	</div>	<p class="tr mt15"><a href="http://movie.mtime.com/104790/details.html">更多详细资料</a><span class="gt">&gt;&gt;</span></p> </div>

        ''.tester
        ''header <h3 class="mt3">更多中文名：</h3>		<ul>		'
        ''rp-middle <li class="mt6">全面启动
        ''rp-middle <span class="c_a5">
        ''rp-optional .....台湾译名
        ''rp-footer </span></li>
        ''.potion
        ''header <h3[\w""'=_ ]*>更多中文名:</h3>(\s*<ul>)?
        ''rp-middle <[\w""'=_ ]+>\s*(?<title>[]+)
        ''rp-middle <[\w""'=_ ]+>
        ''rp-optional [ .…]*(?<area>[]+)?
        ''rp-footer </\w+>{2}
        'revision 1.02 : Match = Regex.Match(mSource, "更多中文名:\s*</h3>(\s*<\w+>)*(\s*<[\w\+\-\|/\\""'&=_ ]+>\s*(?<title>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*<[\w\+\-\|/\\""'&=_ ]+>\s*(?<area>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+|)(\s*</\w+>)+)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "更多中文名:(\s*<((?<!>).)+)+(?<title>\s*<((?!</li>|</ul|</div|<h).)+</(li|ul)>)+", RegexOptions.IgnoreCase)
        Dim strTitleHk As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("title").Captures
                If c.Value.Contains("香港") Then
                    strTitleHk &= Regex.Replace(Regex.Replace(c.Value, "<span((?<!</span>).)+", String.Empty), "(<((?<!>).)+)+", String.Empty).Trim & "/"
                End If
            Next
        End If
        Return charRef.cleanRef(strTitleHk.TrimEnd("/"c))
    End Function

    Public Function getTitleTw() As String
        ''Chinese Title (Taiwan)
        Dim Match As Match = Regex.Match(mSource, "更多中文名:(\s*<((?<!>).)+)+(?<title>\s*<((?!</li>|</ul|</div|<h).)+</(li|ul)>)+", RegexOptions.IgnoreCase)
        Dim strTitleTw As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("title").Captures
                If c.Value.Contains("台湾") Then
                    strTitleTw &= Regex.Replace(Regex.Replace(c.Value, "<span((?<!</span>).)+", String.Empty), "(<((?<!>).)+)+", String.Empty).Trim & "/"
                End If
            Next
        End If
        Return charRef.cleanRef(strTitleTw.TrimEnd("/"c))
    End Function

    Public Function getCompany() As String
        ''Release Company
        '钢铁<strong class="bold">发行公司：</strong>		<a target="_blank" href="/movie/company/132/" class="mr12">派拉蒙影业公司</a>
        '叶问<strong class="bold">发行公司：</strong>		<a target="_blank" href="/movie/company/12002/" class="mr12">东方电影制作有限公司</a>	
        '.tester
        'header <strong class="bold">发行公司：</strong>		'
        'middle <a target="_blank" href="/movie/company/12002/" class="mr12">东方电影制作有限公司</a>	
        '.potion
        'header <[\w""'=_ ]+>发行公司:\s*</\w+>
        'middle <[\w\-\%/""':.=+_ ]+>(?<company>[]+)</\w+>
        'revision 1.02 : Match = charRef.addRef(Regex.Match(mSource, "发行公司:\s*</\w+>\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>\s*(?<company>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)</\w+>", RegexOptions.IgnoreCase).Groups("company").Value.TrimEnd)
        Return charRef.cleanRef(Regex.Match(mSource, "发行公司:(\s*<((?<!>).)+)+(?<company>((?!<).)+)", RegexOptions.IgnoreCase).Groups("company").Value.Trim)
    End Function

    Public Function getCertificate() As String
        ''Certification
        '钢铁<strong class="bold">分级：</strong>	<a target="_blank" href="/movie/section/certification/USA%d0%aePG-13/">	USA:PG-13	</a>
        '叶问<strong class="bold">分级：</strong>	<a target="_blank" href="/movie/section/certification/Taiwan%d0%aePG-12/">	Taiwan:PG-12	</a>
        '全城热恋<strong class="bold">分级：</strong>	<a target="_blank" href="/movie/section/certification/Hong_Kong%d0%aeIIA/">	Hong Kong:IIA	</a>	

        '.tester
        'header <strong class="bold">分级：</strong>	'
        'middle <a target="_blank" href="/movie/section/certification/Hong_Kong%d0%aeIIA/">	'
        'footer Hong Kong:IIA	</a>
        '.potion
        'header <[\w""'=_ ]+>分级:\s*</\w+>
        'middle <[\w\-\%/""':.=+_ ]+>
        'footer Hong Kong:IIA	</\w+>
        'revision 1.02 : Match = Regex.Match(mSource, "分级:\s*</\w+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>\s*(?<certificate>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\|]+)\s*</\w+>(\s*/)*)+", RegexOptions.IgnoreCase)
        Return charRef.cleanRef(Regex.Match(mSource, "分级:(\s*<((?<!>).)+)+(?<cert>((?!<).)+)", RegexOptions.IgnoreCase).Groups("cert").Value.Trim)
    End Function

    Public Function getCountry() As String
        ''Country of Origin
        '钢铁<strong>国家/地区：</strong>		<a target="_blank" href="/movie/section/area/USA/">	美国&nbsp;</a>		</li>	
        '叶问<strong>国家/地区：</strong>		<a target="_blank" href="/movie/section/area/Hong_Kong/">	香港&nbsp;</a>		</li>
        '弹道 http://www.mtime.com/movie/70951/
        '<strong>国家/地区：</strong>		<a target="_blank" href="/movie/section/area/Taiwan/">	台湾&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/area/Hong_Kong/">	香港&nbsp;</a>		</li>

        '.tester
        'header <strong>国家/地区：</strong>		'
        'rp-middle <a target="_blank" href="http://www.mtime.com/ong_Kong/">	香港 </a>
        'rp-optional '	/ 	'
        '.potion
        'header <[\w""'=_ ]+>国家/地区:\s*</\w+>
        'rp-middle <[\w\-\%/""':.=+_ ]+>\s*(?<country>[]+)\s*</\w+>
        'rp-optional (\s*/)?
        'revision 1.02 : Match = Regex.Match(mSource, "国家/地区:\s*</\w+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>\s*(?<country>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*</\w+>(\s*/)*)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "国家/地区:(\s*<((?<!>).)+)+(?<country>\s*((?!</a>|</li|<div).)+</a>\s*[^<]?)+", RegexOptions.IgnoreCase)
        Dim strCountry As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("country").Captures
                strCountry &= Regex.Replace(c.Value, "(<((?<!>).)+|\s|/)", String.Empty) & "/"
            Next
        End If
        Return strCountry.TrimEnd("/"c)
    End Function

    Public Function getGenre() As String
        ''Genre     
        '钢铁<strong class="bold">类型：</strong>		<a target="_blank" href="/movie/section/genre/Thriller/" >	惊悚</a>	/	<a target="_blank" href="/movie/section/genre/Sci-Fi/" >	科幻</a>	/	<a target="_blank" href="/movie/section/genre/Adventure/" >	冒险</a>	/	<a target="_blank" href="/movie/section/genre/Action/" >	动作</a>
        '叶问<strong>类型：</strong>		<a target="_blank" href="/movie/section/genre/Action/">动作&nbsp</a>	/&nbsp;	<a target="_blank" href="/movie/section/genre/History/">历史&nbsp</a>	/&nbsp;	<a target="_blank" href="/movie/section/genre/Biography/">传记&nbsp</a>

        '.tester
        'header <strong class="bold">类型：</strong>		'
        'rp-middle <a target="_blank" href="/movie/section/genre/Thriller/" >	惊悚</a>
        'rp-footer '	/	'
        '.potion
        'header <[\w""'=_ ]+>类型:\s*</\w+>
        'rp-middle <[\w\-\%/""':.=+_ ]+>\s*(?<genre>\[]+)\s*</\w+>
        'rp-footer (\s*/)?
        'revision 1.02 : Match = Regex.Match(mSource, "类型:\s*</\w+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>\s*(?<genre>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*</\w+>(\s*/)*)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "类型:(\s*<((?<!>).)+)+(?<genre>\s*((?!</a>|</li|<div).)+</a>\s*[^<]?)+", RegexOptions.IgnoreCase)
        Dim strGenre As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("genre").Captures
                strGenre &= Regex.Replace(c.Value, "(<((?<!>).)+|\s|/)", String.Empty) & "/"
            Next
        End If
        Return strGenre.TrimEnd("/"c)
    End Function

    Public Function getPremiereDate() As String
        ''Premiere Date & Country
        '钢铁<strong class="bold">上映日期：</strong>	2010年4月27日&nbsp;	中国	&nbsp;<a href="/movie/104696/releaseinfo.html">更多<span class="gt">&gt;&gt;</span></a>
        '叶问<strong class="bold">上映日期：</strong>	2010年5月7日&nbsp;	中国	&nbsp;<a href="/movie/83329/releaseinfo.html">更多<span class="gt">&gt;&gt;</span>
        '杜拉<strong class="bold">上映日期：</strong>	2010年4月15日&nbsp;	中国	</ul>	

        '.tester
        'header <strong class="bold">上映日期：</strong>	'
        'middle 2010年4月27日&nbsp;	中国	'
        'footer <
        '.potion
        'header <[\w""'=_ ]+>上映日期:\s*</\w+>
        'middle (?<year>\d{4})年(?<month>\d{1,2})月((?<date>\d{1,2})日)?\s*(?<country>\w+)
        'footer (<)?
        'revision 1.02 : Match = Regex.Match(mSource, "上映日期:\s*</\w+>\s*(?<year>\d{4})年(?<month>\d{1,2})月(?<date>\d{1,2}|)日?", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "上映日期:(\s*<((?<!>).)+)+\s*(?<year>\d{4})\s*年\s*(?<month>\d{1,2})\s*月\s*(?<date>\d{1,2})?\s*日?", RegexOptions.IgnoreCase)
        Dim strDate As String = String.Empty
        If Match.Success Then
            strDate = Match.Groups("year").Value & "年" & Match.Groups("month").Value.TrimStart("0"c) & "月" & _
            IIf(Match.Groups("date").Success, Match.Groups("date").Value.TrimStart("0"c) & "日", "")
        End If
        Return strDate
    End Function

    Public Function getPremiereCountry() As String
        ''Premiere Date & Premiere Country
        '钢铁<strong class="bold">上映日期：</strong>	2010年4月27日&nbsp;	中国	&nbsp;<a href="/movie/104696/releaseinfo.html">更多<span class="gt">&gt;&gt;</span></a>
        '叶问<strong class="bold">上映日期：</strong>	2010年5月7日&nbsp;	中国	&nbsp;<a href="/movie/83329/releaseinfo.html">更多<span class="gt">&gt;&gt;</span>
        '杜拉<strong class="bold">上映日期：</strong>	2010年4月15日&nbsp;	中国	</ul>	
        '杜拉<strong class="bold">上映日期：</strong> <span property="v:initialReleaseDate" content="2010-4-15">	2010年4月15日 </span>  	中国	</ul>	</div>	<!-- 预告片 -->		<div class="fr w255 o_h">	<div class="clearfix">	<em class="fr"><a href="http://movie.mtime.com/104453/trailer.html">
        '.tester
        'header <strong class="bold">上映日期：</strong>	'
        'middle 2010年4月27日&nbsp;	中国	'
        'footer <
        '.potion
        'header <[\w""'=_ ]+>上映日期:\s*</\w+>
        'middle (?<year>\d{4})年(?<month>\d{1,2})月((?<date>\d{1,2})日)?\s*(?<country>\w+)
        'footer (<)?
        'revision 1.02 : Match = Regex.Match(mSource, "上映日期:\s*</\w+>\s*\d{4}年\d{1,2}月(\d{1,2}日)?\s*(?<country>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*<", RegexOptions.IgnoreCase).Groups("country").Value.TrimEnd
        Return Regex.Match(mSource, "上映日期:(\s*<((?<!>).)+)+((?<!月).)+(\s*\d{1,2}日)?(\s*<((?<!>).)+)*(?<country>((?!<a|</ul|<span|</li|<div).)+)", RegexOptions.IgnoreCase).Groups("country").Value.Trim
    End Function

    Public Function getOfficialSite() As String
        ''Official Website
        '钢铁<strong class="bold">官方网站：</strong>		<a target="_blank" href="http://www.ipman2-movie.com/cn_main.html" class="mr12">官方网站</a>
        '叶问<strong class="bold">官方网站：</strong>		<a target="_blank" href="http://ironmanmovie.marvel.com/" class="mr12">Marvel [us]</a>

        '.tester
        'header <strong class="bold">官方网站：</strong>		'
        'middle <a target="_blank" href="http://www.ipman2-movie.com/cn_main.html" class="mr12">官方网站
        'footer </a>
        '.potion
        'header <[\w""'=_ ]+>官方(网站|博客):\s*</\w+>
        'middle <a[\w""'=_ ]+href=""(?<offsite>http://[\w\-\%/.=_]+)"" ?[\w""'=_ ]+*>"
        'revision 1.02 : Match = Regex.Match(mSource, "官方(网站|博客):\s*</\w+>\s*<a [\w\+\-/\\""'&=_ ]*href=""(?<offsite>[\w\+\-\.\?/\\'#%&:;=_]+)""", RegexOptions.IgnoreCase).Groups("offsite").Value
        Return Regex.Match(mSource, "官方(网站|博客):(\s*<((?<!<a ).)+)+((?<!href="").)+(?<site>((?!"").)+)", RegexOptions.IgnoreCase).Groups("site").Value
    End Function

    Public Function getLanguage() As String
        ''Language
        '钢铁<strong>对白语言：</strong>		<a target="_blank" href="/movie/section/language/English/">	英语&nbsp;</a>
        '叶问<strong>对白语言：</strong>		<a target="_blank" href="/movie/section/language/Cantonese/">	粤语&nbsp;</a>
        '全城热恋 114616
        '<strong>对白语言：</strong>		<a target="_blank" href="/movie/section/language/Cantonese/">	粤语&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/Mandarin/">	汉语普通话&nbsp;</a>		</li>
        '艋舺 105754
        '<strong>对白语言：</strong>		<a target="_blank" href="/movie/section/language/Mandarin/">	汉语普通话&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/Min+Nan/">	Min Nan&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/Chinese/">	Chinese&nbsp;</a>		</li>
        '拯救大兵瑞恩 12622
        '<strong>对白语言：</strong>		<a target="_blank" href="/movie/section/language/Czech/">	捷克语&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/English/">	英语&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/French/">	法语&nbsp;</a>	/&nbsp;	<a target="_blank" href="/movie/section/language/German/">	德语&nbsp;</a>		</li>

        '.tester
        'header <strong>对白语言：</strong>		'
        'middle <a target="_blank" href="/movie/section/language/Czech/">	捷克语&nbsp;</a>
        'footer '	/&nbsp;	
        '.potion
        'header <[\w""'=_ ]+>对白语言:\s*</\w+>
        'middle <[\w\-\%/""':.=+_ ]+>\s*(?<language>\[]+)\s*</\w+>
        'footer (\s*/)?
        'revision 1.02 : Match = Regex.Match(mSource, "对白语言:\s*</\w+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>\s*(?<language>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*</\w+>(\s*/)*)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "对白语言:(\s*<((?<!>).)+)+(?<lua>\s*((?!</a>|</li|<div).)+</a>\s*[^<]?)+", RegexOptions.IgnoreCase)
        Dim strLanguage As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("lua").Captures
                strLanguage &= Regex.Replace(c.Value, "(<((?<!>).)+|\s|/)", String.Empty) & "/"
            Next
        End If
        Return strLanguage.TrimEnd("/"c)
    End Function

    Public Function getRuntime() As String
        ''Runtime
        '钢铁<strong>片长：</strong>124 min</li>
        '叶问<strong>片长：</strong>108分钟</li>	

        '.tester
        'header <strong>片长：</strong>	
        'middle 108分钟</li>
        '.potion
        'header <[\w""'=_ ]+>片长:\s*</\w+>
        'middle (?<runtime>\d{1,3})\s*\w+\s*</\w+>
        '"片长:\s*</\w+>(?<runtime>\d{1,3})[\w\s]+</\w+>"
        'revision 1.02 : Match = Regex.Match(mSource, "片长:\s*</\w+>(?<runtime>\d{1,3})[\w\s]+</\w+>", RegexOptions.IgnoreCase).Groups("runtime").Value
        Return Regex.Match(mSource, "片长:(\s*<((?<!>).)+)+(?<dur>\d{1,3})", RegexOptions.IgnoreCase).Groups("dur").Value
    End Function

    Public Function getYear() As String
        ''Year
        'revision 1.36 : 
        'Ganja & Hess
        '<a href="/movie/70374/" class="ml9 px24">Ganja & Hess</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/movie/section/year/1973/" >1973</a>)<span class="ml9 px14"></span></em></h1>
        'Iron Man 2
        '<a href="/movie/83329/" class="px28 bold hei c_000">钢铁侠2</a><a href="/83329/" class="ml9 px24">Iron Man 2</a> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="/movie/section/year/2010/" >2010</a>)<span class="ml9 px14"></span></em></h1>
        'revision 1.37 : 
        'Ganja & Hess
        '<span class="ml9 px24">Ganja & Hess</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=1973" >1973</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>
        'Iron Man 2
        '<span class="px28 bold hei c_000" property="v:itemreviewed">钢铁侠2</span><span class="ml9 px24">Iron Man 2</span> <em class="px18 c_666 bold ml9"> (<a class="c_666" href="http://movie.mtime.com/movie/search/section/?year=2010" >2010</a>)	<span class="ml9 px14"></span></em>	<span id="followButtonRegion" class="mt-3 p_r"></span>	</h1>

        'revision 1.02 : N/A
        'revision 1.36 : Regex "section/year/(?<year>\d{4})"
        'revision 1.36 : Regex "section/\?year=(?<year>\d{4})"
        Return Regex.Match(mSource, "section/\?year=(?<year>\d{4})", RegexOptions.IgnoreCase).Groups("year").Value
    End Function
#End Region

#Region "> ======== CREDIT SECTION ========"
    Public Function getDirector() As String
        ''Director
        '全城热恋 114616
        '<h3 id="Director" class="lh20 px14 mt30">导演 Director：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li> vbNewLine <a href="/person/903078/">夏永康</a>&nbsp;<a href="/person/903078/">Wing Shya</a></li><li>
        '<a href="/person/1044682/">陈国辉</a>&nbsp;<a href="/person/1044682/">Tony Chan</a></li></ul>
        '弹道 70951
        '<h3 id="Director" class="lh20 px14 mt30">导演 Director：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li> vbNewLine <a href="/person/893068/">刘国昌</a>&nbsp;<a href="/person/893068/">Lawrence Ah Mon</a></li></ul>
        '小叮当 85489l
        '<h3 id="Director" class="lh20 px14 mt30">导演 Director：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li> vbNewLine <a href="/person/901240/">Bradley Raymond</a></li></ul>
        '杜拉拉
        '<h3 id="Director" class="lh20 px14 mt30">导演 Director：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li> vbNewLine <a href="/person/892974/">徐静蕾</a>&nbsp;<a href="/person/892974/">Jinglei Xu</a></li><li>
        '<a href="/person/1249086/">安韩瑾</a>&nbsp;<a href="/person/1249086/">Hanjin An</a>&nbsp;&nbsp;<em class="c_a5 ml6">....执行导演</em></li></ul>

        '.tester
        'header <h3 id="Director" class="lh20 px14 mt30">导演 Director：</h3>
        'header <div class="line_dot"></div><ul class="staff_actor_list">
        'rp-header <li>
        'rp-middle <a href="/person/1044682/">陈国辉</a>
        'rp-optional <a href="/person/1044682/">Tony Chan</a>
        'rp-optional <em class="c_a5 ml6">....执行导演</em>
        'rp-footer </li>
        'footer </ul>
        '.potion
        'header <h3 [\w\+\-\|/\\""'&=_ ]+>导演 Director:\s*</h3>
        'header (\s*<div [\w\+\-\|/\\""'&=_ ]+>\s*</div>)?(\s*<ul [\w\+\-\|/\\""'&=_ ]+>)?[\r\n\s]*
        'rp-header <\w+>
        'rp-middle <[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>(?<director>[zh]+)\s*</\w+>
        'rp-optional <[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>(?<director>[en]+)\s*</\w+>
        'rp-optional <[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>(?<director>[ex]+)\s*</\w+>
        'rp-footer </\w+>
        'footer </\w+>
        'revision 1.02 : Match = Regex.Match(mSource, "导演 Director:\s*</h3>(\s*<div [\w\+\-\|/\\""'&=_ ]+>\s*</div>)?(\s*<ul [\w\+\-\|/\\""'&=_ ]+>)?\s*(?<director>\s*<\w+>(\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+\s*</\w+>){1,3}\s*</\w+>)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "导演 Director:(\s*<((?<!>).)+)+(?<director>(\s*(?!</li|</ul|<h).)+</li>)+", RegexOptions.IgnoreCase)
        Dim strDirector As String = String.Empty
        If Match.Success Then
            For Each c As Capture In Match.Groups("director").Captures
                strDirector &= Regex.Replace(c.Value, "(<((?<!>).)+)+", String.Empty).Trim & vbNewLine
            Next
        End If
        Return charRef.cleanRef(strDirector.TrimEnd)
    End Function

    Public Function getCast() As String
        ''Cast
        '全城热恋 114616
        '<h3 id="Actor" class="lh20 px14 mt30">演员 Actor：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li ><a href="/person/924883/"><img alt="Jacky Cheung" src="http://img1.mtime.com/ph/883/924883/924883_22X22.jpg"/></a><a href="/person/924883/">张学友</a>&nbsp;<a href="/person/924883/">Jacky Cheung</a></li><li class='dark'><a href="/person/904631/"><img alt="Rene Liu" src="http://img1.mtime.com/ph/631/904631/904631_22X22.jpg"/></a><a href="/person/904631/">刘若英</a>&nbsp;<a href="/person/904631/">Rene Liu</a></li>
        '弹道 70951
        '<h3 id="Actor" class="lh20 px14 mt30">演员 Actor：</h3><div class="line_dot"></div><ul class="staff_actor_list">
        '<li ><a href="/person/916259/"><img alt="Simon Yam" src="http://img21.mtime.cn/ph/259/916259/916259_22X22.jpg"/></a><a href="/person/916259/">任达华</a>&nbsp;<a href="/person/916259/">Simon Yam</a>&nbsp;&nbsp;<em class="c_a5 ml6">....孙学人</em></li><li class='dark'><a href="/person/1249295/"><img alt="Hsiao-chuan Chang" src="http://img21.mtime.cn/ph/1295/1249295/1249295_22X22.jpg"/></a><a href="/person/1249295/">张孝全</a>&nbsp;<a href="/person/1249295/">Hsiao-chuan Chang</a>&nbsp;&nbsp;<em class="c_a5 ml6">....徐瑜昌</em></li><li ><a href="/person/1100344/"><img alt="Kog Jung-Xung" src="http://img21.mtime.cn/ph/344/1100344/1100344_22X22.jpg"/></a><a href="/person/1100344/">柯俊雄</a>&nbsp;<a href="/person/1100344/">Kog Jung-Xung</a>&nbsp;&nbsp;<em class="c_a5 ml6">....庞天南</em></li><li class='dark'><a href="/person/966029/"><img alt="Guozhu Zhang" src="http://img21.mtime.cn/ph/29/966029/966029_22X22.jpg"/></a><a href="/person/966029/">张国柱</a>&nbsp;<a href="/person/966029/">Guozhu Zhang</a>&nbsp;&nbsp;<em class="c_a5 ml6">....方正北</em></li><li ><a href="/person/892836/"><img alt="Leon Dai" src="http://img21.mtime.cn/ph/836/892836/892836_22X22.jpg"/></a><a href="/person/892836/">戴立忍</a>&nbsp;<a href="/person/892836/">Leon Dai</a></li><li class='dark'><a href="/person/924891/"><img alt="Ka Tung Lam" src="http://img21.mtime.cn/ph/891/924891/924891_22X22.jpg"/></a><a href="/person/924891/">林家栋</a>&nbsp;<a href="/person/924891/">Ka Tung Lam</a></li><li ><a href="/person/924271/"><img alt="Kai Chi Liu" src="http://img21.mtime.cn/ph/271/924271/924271_22X22.jpg"/></a><a href="/person/924271/">廖启智</a>&nbsp;<a href="/person/924271/">Kai Chi Liu</a>&nbsp;&nbsp;<em class="c_a5 ml6">....陈二同</em></li><li class='dark'><a href="/person/1173647/"><img alt="Chung Kwan Wong" src="http://img21.mtime.cn/ph/1647/1173647/1173647_22X22.jpg"/></a><a href="/person/1173647/">黄仲昆</a>&nbsp;<a href="/person/1173647/">Chung Kwan Wong</a></li><li ><a href="/person/1260131/"><img alt="Mengsheng Shen" src="http://img21.mtime.cn/ph/131/1260131/1260131_22X22.jpg"/></a><a href="/person/1260131/">沈孟生</a>&nbsp;<a href="/person/1260131/">Mengsheng Shen</a>&nbsp;&nbsp;<em class="c_a5 ml6">....吴立雄</em></li><li class='dark'><a href="/person/1518354/"><img alt="Jiwei Fang" src="http://img21.mtime.cn/ph/354/1518354/1518354_22X22.jpg"/></a><a href="/person/1518354/">方季惟</a>&nbsp;<a href="/person/1518354/">Jiwei Fang</a></li><li ><a href="/person/1088403/"><img alt="Don Wong" src="http://img21.mtime.cn/ph/403/1088403/1088403_22X22.jpg"/></a><a href="/person/1088403/">王道</a>&nbsp;<a href="/person/1088403/">Don Wong</a></li><li class='dark'><a href="/person/1035179/"><img alt="Zhang Han" src="http://img21.mtime.cn/ph/2010/05/09/130641.66226551_22X22.jpg"/></a><a href="/person/1035179/">张翰</a>&nbsp;<a href="/person/1035179/">Zhang Han</a></li><li ><a href="/person/930587/"><img alt="Ting-Ting Hu" src="http://img21.mtime.cn/ph/587/930587/930587_22X22.jpg"/></a><a href="/person/930587/">胡婷婷</a>&nbsp;<a href="/person/930587/">Ting-Ting Hu</a></li><li class='dark'><a href="/person/1427468/"><img alt="Kaixuan Tseng" src="http://img21.mtime.cn/ph/1468/1427468/1427468_22X22.jpg"/></a><a href="/person/1427468/">曾恺玹</a>&nbsp;<a href="/person/1427468/">Kaixuan Tseng</a>&nbsp;&nbsp;<em class="c_a5 ml6">....陈二同之女</em></li></ul>

        '全城热恋 sorted
        '<h3 id="Actor" class="lh20 px14 mt30">演员 Actor：</h3><div class="line_dot"></div><ul class="staff_actor_list"> vbnewline <li >
        '<a href="924883/"><img alt="Jacky Cheung" src="http://22X22.jpg"/></a><a href="/person/924883/">张学友</a>&nbsp;<a href="/person/924883/">Jacky Cheung</a></li><li class='dark'>
        '<a href="904631/"><img alt="Rene Liu" src="http://22X22.jpg"/></a><a href="/person/904631/">刘若英</a>&nbsp;<a href="/person/904631/">Rene Liu</a></li><li >
        '<a href="903079/"><img alt="Nicholas Tse" src="http://22X22.jpg"/></a><a href="/person/903079/">谢霆锋</a>&nbsp;<a href="/person/903079/">Nicholas Tse</a></li><li class='dark'>
        '</li></ul>

        '弹道 sorted
        '<h3 id="Actor" class="lh20 px14 mt30">演员 Actor：</h3><div class="line_dot"></div><ul class="staff_actor_list">  vbnewline <li >
        '<a href="9162590/"><img alt="Simon Yam" src="http://22X22.jpg"/></a><a href="/916259/">任达华</a>&nbsp;<a href="/person/916259/">Simon Yam</a>&nbsp;&nbsp;<em class="c_a5 ml6">....孙学人</em></li><li class='dark'>
        '<a href="1249295/"><img alt="Hsiao-chuan Chang" src="http://22X22.jpg"/></a><a href="/1249295/">张孝全</a>&nbsp;<a href="/person/1249295/">Hsiao-chuan Chang</a>&nbsp;&nbsp;<em class="c_a5 ml6">....徐瑜昌</em></li><li >
        '<a href="1100344/"><img alt="Kog Jung-Xung" src="22X22.jpg"/></a><a href="1100344/">柯俊雄</a>&nbsp;<a href="1100344/">Kog Jung-Xung</a>&nbsp;&nbsp;<em class="c_a5 ml6">....庞天南</em></li><li class='dark'>
        '<a href="892836/"><img alt="Leon Dai" src="http://22X22.jpg"/></a><a href="/person/892836/">戴立忍</a>&nbsp;<a href="/person/892836/">Leon Dai</a></li><li class='dark'>
        '</li></ul>

        'Non-english Name
        '<a href="1259868/"><img alt="" src="http://22X22.jpg"/></a><a href="1259868/">Pamela Adlon</a>&nbsp;&nbsp;<em class="c_a5 ml6">....Vidia (voice)</em></li><li class='dark'>

        '.tester
        'header <h3 id="Actor" class="lh20 px14 mt30">演员 Actor：</h3>
        'header <div class="line_dot"></div><ul class="staff_actor_list">
        'rp-header vbnewline <li > or <li class='dark'>
        'rp-middle <a href="/person/9162590/"><img alt="Simon Yam" src="http://22X22.jpg"/></a>
        'rp-middle <a href="/person/916259/">任达华</a>
        'rp-optional<a href="/person/916259/">Simon Yam</a>
        'rp-optional<em class="c_a5 ml6">....孙学人</em>
        'rp-footer </li>
        'footer </ul>
        '.potion
        'header <h3 [\w\+\-\|/\\""'&=_ ]+>演员 Actor:\s*</h3>
        'header (\s*<div [\w\+\-\|/\\""'&=_ ]+>\s*</div>)?(\s*<ul [\w\+\-\|/\\""'&=_ ]+>)?[\r\n\s]*
        'rp-header (\s*<[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>)+
        'rp-middle ''
        'rp-middle (?<name>[]+)</\w+>)
        'rp-optional (?<char>(<a[\w\+\-\.\?\|/\\""'#%&:;=_ ]+>[]+</\w+>)|)
        'rp-optional (?<em>(<[\w\+\-\|/\\""'&=_ ]+>[]+</\w+>)|)
        'rp-footer </\w+>
        'footer </\w+>
        'revision 1.02 : Match = Regex.Match(mSource, "演员 Actor:\s*</h3>(\s*<div [\w\+\-/\\""'&=_ ]+>\s*</div>)?(\s*<ul [\w\+\-/\\""'&=_ ]+>)?\s*((\s*<[\w\+\-\.\?/\\""'#%&:;=_ ]+>)+\s*(?<name>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+)\s*</\w+>\s*(?<char>(<a[\w\+\-\.\?/\\""'#%&:;=_ ]+>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+</\w+>)|)\s*(?<em>(<[\w\+\-/\\""'&=_ ]+>[\w""'`!#$%&*,:;=@~/\\_\+\-\.\?\^\(\)\[\]\{\}\| ]+</\w+>)|)\s*</\w+>)+", RegexOptions.IgnoreCase)
        Dim Match As Match = Regex.Match(mSource, "演员 Actor:(\s*<((?<!>).)+)+(?<cast>(\s*(?!</li|</ul|<h).)+</li>)+", RegexOptions.IgnoreCase)
        Dim strCast As String = String.Empty
        If Match.Success Then
            For gCtr As Integer = 0 To Match.Groups("cast").Captures.Count - 1
                strCast &= Regex.Replace(Match.Groups("cast").Captures.Item(gCtr).Value, "(<((?<!>).)+)+", String.Empty).Trim & vbNewLine
                If gCtr >= 14 Then : Exit For : End If
            Next
        End If
        Return charRef.cleanRef(strCast.TrimEnd)
    End Function
#End Region

#Region "> ======== PLOT SECTION ========"
    Public Function getPlot() As String
        '.tester
        'header <div class=\"juqing\">\t<p class=\"mt20\">\t<P>
        'rp-optional </P>\t</p>\t<p >\t<P>
        'rp-optional <A href=\"http://www.mtime.com/person/924319/\" target=_blank><FONT color=#2f688c>
        'rp-middle <span class=\"m_s60\">Word</span>
        'rp-optional </FONT></A>
        're-footer'.potion
        'header <div [\w""'=+-_ ]+>(\\t)?
        'rp-optional (?<break>(</?p[]+>(\\t)?</?p ?>)*|)
        'rp-optional (<a [\w""'=+-_ ]+><[\w#= ]+>)
        'rp-middle <\w+ class=\\""(?<class>[\w_]+)\\"">
        'rp-middle (?<word>[]+)</\w+>(?<movie>([]+|)</\w+>
        'rp-optional </font></a>
        're-footer
        Dim Matches As MatchCollection = _
        Regex.Matches(mSource, "div((?<!>).)+(?<ident>((?!</div>).)+)", RegexOptions.IgnoreCase)
        If Matches.Count >= 1 Then
            Dim longest As Integer = 0
            If Not Matches.Count = 1 Then
                Dim length As Int64 = 0
                For mCtr As Integer = 0 To Matches.Count - 1
                    If length <= Matches.Item(mCtr).Value.Length Then
                        length = Matches.Item(mCtr).Value.Length
                        longest = mCtr : End If
                Next
            End If

            If Matches.Item(longest).Groups("ident").Value.Length > 150 Then
                Dim strPlot As String = Regex.Replace( _
                Matches.Item(longest).Groups("ident").Value, "<span class=\\""(?<class>((?!\\"").)+)\\"">(?<word>((?!</span>).)+)</span>", AddressOf scCleaner, RegexOptions.IgnoreCase)
                strPlot = Regex.Replace( _
                strPlot, "(<(/?p|br)[^<>]*>?|\\n|\\t)(\s|</?\w[^<>]*>?|\\n|\\t|\u00a0|\u3000)*", _
                vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288), RegexOptions.IgnoreCase)
                Return charRef.cleanRef(ChrW(12288) & ChrW(12288) & Regex.Replace(strPlot, "<((?<!>).)+", String.Empty).Trim)
            Else : Return String.Empty : End If
        Else
            Return String.Empty
        End If
    End Function
#End Region

#Region "> ======== EXTRAS SECTION ========"
    ' 20110106 Revision : 1.32
    Public Function getExtras() As String
        '.tester
        'header <h3 class=\"lh20 px14 mt30\" id=\"ProductionNotes\">
        'optional <a href=\"/movie/102279/addMovieInfo.html\" class=\"fr btn_yellow\">我来添加</a>
        'header 幕后制作 </h3>
        'header <div class=\"line_dot\"></div>  '
        'rp-header <p class=\"mt12\"> <P> or <Br>
        'rp-optional <STRONG> or <A href=\"http://www.mtime.com/person/916259\" target=_blank>
        'rp-middle <span class=\"m_s16\">任
        'rp-middle </span>ABC or <BR>
        'rp-optional </A> or </STRONG>
        'rp-optional <p class="">ABC<P>
        'footer </P> </p>

        '.potion
        'header <h3 []+>
        'optional (\s*<a []+>[]+</a>)?
        'header \s*(?<head>[]+)\s*</h3>
        'header (\s*</?div[\w\+\-/\\""'&=_ ]*>)*
        'rp-header (?<break>(\s*<p[]*>)+|(\s*<br ?/?>)+|)
        'rp-optional (\s|<strong>|<a []+>|<font []+>)*
        'rp-middle <\w+ class=\\""(?<class>[]+)\\"">(?<word>[]+)
        'rp-middle </\w+>(?<alpha>((<br ?/?>)+|[]+|)*)
        'rp-optional (\s|</strong>|</a>|</font>|
        'rp-optional <p class="">(?<remark>[]+)<P>
        'footer </p>)*)+
        Dim Matches As MatchCollection = _
        Regex.Matches(mSource, "h3((?<!>).)+(\s*<a((?!</a>).)+</a>)?\s*(?<head>((?!</h3>).)+)</h3>(\s*<div((?!</div>).)+</div>)*(?<content>\s*((?!<h|</div).)+)", RegexOptions.IgnoreCase)
        If Matches.Count >= 1 Then
            Dim strExtras As New StringBuilder

            For Each m As Match In Matches
                Dim exContent As String = Regex.Replace(m.Groups("content").Value, "<span class=\\""(?<class>((?!\\"").)+)\\"">(?<word>((?!</span>).)+)</span>", AddressOf scCleaner, RegexOptions.IgnoreCase)
                If m.Groups("head").Value.Contains("一句话评论") Then
                    'if <p class=""> follow not same instance of <p class="">, remove (<>){2}
                    exContent = Regex.Replace( _
                    Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
                    "<p class=(?<class>((?!>).)+)>(?<content>((?!<p class=\k<class>|<h3|</div).)+)", _
                    AddressOf alignReview, RegexOptions.IgnoreCase)
                    'revision 1.02 :
                    'exContent = Regex.Replace( _
                    'Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
                    '"(?<review><p class=(?<classA>((?!>).)+)>((?!</p>).)+)(</p>\s*)+<p class=(?<classB>((?!>).)+)>" & _
                    '"(?<author>((?<!</p>).)+)", AddressOf alignReview, RegexOptions.IgnoreCase)
                Else
                    exContent = Regex.Replace(exContent, "(作者|发布者):((?!</p>).)+", String.Empty, RegexOptions.IgnoreCase)

                    exContent = Regex.Replace( _
                    Regex.Replace(exContent, "(<br ?/?>\s*)+</(b|strong)>\s*", "[/b]<br><br>", RegexOptions.IgnoreCase), _
                    "</(b|strong)>(\s*<br ?/?>)+\s*", "[/b]<br><br>", RegexOptions.IgnoreCase)

                    exContent = Regex.Replace( _
                    Regex.Replace(exContent, "<(b|strong)>\s*", "[b]", RegexOptions.IgnoreCase), _
                    "\s*</(b|strong)>", "[/b]", RegexOptions.IgnoreCase)


                    exContent = Regex.Replace( _
                    Regex.Replace(exContent, "<br ?/?>(?!<br ?/?>)(\s|\u00a0|\u3000|\\\w)*", vbNewLine & ChrW(12288) & ChrW(12288), _
                                  RegexOptions.IgnoreCase), "((\u00a0|\u3000|\\\w)+\r\n)+", vbNewLine)

                    If Not m.Groups("head").Value.Contains("精彩对白") Then
                        exContent = Regex.Replace(exContent, "(\r\n(\u00a0|\u3000|\\\w)+)+", vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288))
                    End If

                    End If

                strExtras.Append("[b]" & m.Groups("head").Value.TrimEnd & "[/b]" & vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288) & Regex.Replace( _
                                exContent, "(<(/?p|br)[^<>]*>?|\\\w)(\s|</?\w[^<>]*>?|\\\w|\u00a0|\u3000)*", _
                                 vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288), RegexOptions.IgnoreCase).Trim & vbNewLine & vbNewLine)
            Next
            Return charRef.cleanRef(Regex.Replace(strExtras.ToString, "<((?<!>).)+", String.Empty).TrimEnd)
        Else
            Return String.Empty
        End If
    End Function

    ' 20110105 Revision : 1.31
    'Public Function getExtras() As String
    '    '.tester
    '    'header <h3 class=\"lh20 px14 mt30\" id=\"ProductionNotes\">
    '    'optional <a href=\"/movie/102279/addMovieInfo.html\" class=\"fr btn_yellow\">我来添加</a>
    '    'header 幕后制作 </h3>
    '    'header <div class=\"line_dot\"></div>  '
    '    'rp-header <p class=\"mt12\"> <P> or <Br>
    '    'rp-optional <STRONG> or <A href=\"http://www.mtime.com/person/916259\" target=_blank>
    '    'rp-middle <span class=\"m_s16\">任
    '    'rp-middle </span>ABC or <BR>
    '    'rp-optional </A> or </STRONG>
    '    'rp-optional <p class="">ABC<P>
    '    'footer </P> </p>

    '    '.potion
    '    'header <h3 []+>
    '    'optional (\s*<a []+>[]+</a>)?
    '    'header \s*(?<head>[]+)\s*</h3>
    '    'header (\s*</?div[\w\+\-/\\""'&=_ ]*>)*
    '    'rp-header (?<break>(\s*<p[]*>)+|(\s*<br ?/?>)+|)
    '    'rp-optional (\s|<strong>|<a []+>|<font []+>)*
    '    'rp-middle <\w+ class=\\""(?<class>[]+)\\"">(?<word>[]+)
    '    'rp-middle </\w+>(?<alpha>((<br ?/?>)+|[]+|)*)
    '    'rp-optional (\s|</strong>|</a>|</font>|
    '    'rp-optional <p class="">(?<remark>[]+)<P>
    '    'footer </p>)*)+
    '    Dim Matches As MatchCollection = _
    '    Regex.Matches(mSource, "h3((?<!>).)+(\s*<a((?!</a>).)+</a>)?\s*(?<head>((?!</h3>).)+)</h3>(\s*<div((?!</div>).)+</div>)*(?<content>\s*((?!<h|</div).)+)", RegexOptions.IgnoreCase)
    '    If Matches.Count >= 1 Then
    '        Dim strExtras As New StringBuilder

    '        For Each m As Match In Matches
    '            Dim exContent As String = Regex.Replace(m.Groups("content").Value, "<span class=\\""(?<class>((?!\\"").)+)\\"">(?<word>((?!</span>).)+)</span>", AddressOf scCleaner, RegexOptions.IgnoreCase)
    '            If m.Groups("head").Value.Contains("一句话评论") Then
    '                'if <p class=""> follow not same instance of <p class="">, remove (<>){2}
    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
    '                "<p class=(?<class>((?!>).)+)>(?<content>((?!<p class=\k<class>|<h3|</div).)+)", _
    '                AddressOf alignReview, RegexOptions.IgnoreCase)
    '                'revision 1.02 :
    '                'exContent = Regex.Replace( _
    '                'Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
    '                '"(?<review><p class=(?<classA>((?!>).)+)>((?!</p>).)+)(</p>\s*)+<p class=(?<classB>((?!>).)+)>" & _
    '                '"(?<author>((?<!</p>).)+)", AddressOf alignReview, RegexOptions.IgnoreCase)
    '            Else
    '                exContent = Regex.Replace(exContent, "(作者|发布者):((?!</p>).)+", String.Empty, RegexOptions.IgnoreCase)

    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "(<br ?/?>\s*)+</(b|strong)>\s*", "[/b]<br><br>", RegexOptions.IgnoreCase), _
    '                "</(b|strong)>(\s*<br ?/?>)+\s*", "[/b]<br><br>", RegexOptions.IgnoreCase)

    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "<(b|strong)>\s*", "[b]", RegexOptions.IgnoreCase), _
    '                "\s*</(b|strong)>", "[/b]", RegexOptions.IgnoreCase)

    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "<br ?/?>(?!<br ?/?>)(\s|\u00a0|\u3000|\\\w)*", vbNewLine & ChrW(12288) & ChrW(12288), _
    '                              RegexOptions.IgnoreCase), "((\u00a0|\u3000|\\\w)+\r\n)+", vbNewLine)

    '                exContent = Regex.Replace(exContent, "(\r\n(\u00a0|\u3000|\\\w)+)+", vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288))

    '            End If
    '            strExtras.Append("[b]" & m.Groups("head").Value.TrimEnd & "[/b]" & vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288) & Regex.Replace( _
    '                             exContent, "(<(/?p|br)[^<>]*>?|\\\w)(\s|</?\w[^<>]*>?|\\\w|\u00a0|\u3000)*", _
    '                             vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288), RegexOptions.IgnoreCase).Trim & vbNewLine & vbNewLine)
    '        Next
    '        Return charRef.cleanRef(Regex.Replace(strExtras.ToString, "<((?<!>).)+", String.Empty).TrimEnd)
    '    Else
    '        Return String.Empty
    '    End If
    'End Function

    ' 20101225 Revision : 1.30
    'Public Function getExtras() As String
    '    '.tester
    '    'header <h3 class=\"lh20 px14 mt30\" id=\"ProductionNotes\">
    '    'optional <a href=\"/movie/102279/addMovieInfo.html\" class=\"fr btn_yellow\">我来添加</a>
    '    'header 幕后制作 </h3>
    '    'header <div class=\"line_dot\"></div>  '
    '    'rp-header <p class=\"mt12\"> <P> or <Br>
    '    'rp-optional <STRONG> or <A href=\"http://www.mtime.com/person/916259\" target=_blank>
    '    'rp-middle <span class=\"m_s16\">任
    '    'rp-middle </span>ABC or <BR>
    '    'rp-optional </A> or </STRONG>
    '    'rp-optional <p class="">ABC<P>
    '    'footer </P> </p>

    '    '.potion
    '    'header <h3 []+>
    '    'optional (\s*<a []+>[]+</a>)?
    '    'header \s*(?<head>[]+)\s*</h3>
    '    'header (\s*</?div[\w\+\-/\\""'&=_ ]*>)*
    '    'rp-header (?<break>(\s*<p[]*>)+|(\s*<br ?/?>)+|)
    '    'rp-optional (\s|<strong>|<a []+>|<font []+>)*
    '    'rp-middle <\w+ class=\\""(?<class>[]+)\\"">(?<word>[]+)
    '    'rp-middle </\w+>(?<alpha>((<br ?/?>)+|[]+|)*)
    '    'rp-optional (\s|</strong>|</a>|</font>|
    '    'rp-optional <p class="">(?<remark>[]+)<P>
    '    'footer </p>)*)+
    '    Dim Matches As MatchCollection = _
    '    Regex.Matches(mSource, "h3((?<!>).)+(\s*<a((?!</a>).)+</a>)?\s*(?<head>((?!</h3>).)+)</h3>(\s*<div((?!</div>).)+</div>)*(?<content>\s*((?!<h|</div).)+)", RegexOptions.IgnoreCase)
    '    If Matches.Count >= 1 Then
    '        Dim strExtras As New StringBuilder

    '        For Each m As Match In Matches
    '            Dim exContent As String = Regex.Replace(m.Groups("content").Value, "<span class=\\""(?<class>((?!\\"").)+)\\"">(?<word>((?!</span>).)+)</span>", AddressOf scCleaner, RegexOptions.IgnoreCase)
    '            exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "<(b|strong)>\s*", "[b]", RegexOptions.IgnoreCase), _
    '                "</(b|strong)>", "[/b]", RegexOptions.IgnoreCase)
    '            If m.Groups("head").Value.Contains("一句话评论") Then
    '                'if <p class=""> follow not same instance of <p class="">, remove (<>){2}
    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
    '                "<p class=(?<class>((?!>).)+)>(?<content>((?!<p class=\k<class>|<h3|</div).)+)", _
    '                AddressOf alignReview, RegexOptions.IgnoreCase)
    '                'revision 1.02 :
    '                'exContent = Regex.Replace( _
    '                'Regex.Replace(exContent, "<br ?/?>", String.Empty, RegexOptions.IgnoreCase), _
    '                '"(?<review><p class=(?<classA>((?!>).)+)>((?!</p>).)+)(</p>\s*)+<p class=(?<classB>((?!>).)+)>" & _
    '                '"(?<author>((?<!</p>).)+)", AddressOf alignReview, RegexOptions.IgnoreCase)
    '            Else
    '                exContent = Regex.Replace( _
    '                Regex.Replace(exContent, "(作者|发布者):((?!</p>).)+", String.Empty, RegexOptions.IgnoreCase), _
    '                "<br ?/?>(\s|\u00a0|\u3000)*", vbNewLine & ChrW(12288) & ChrW(12288), RegexOptions.IgnoreCase)
    '            End If
    '            strExtras.Append("[b]" & m.Groups("head").Value.TrimEnd & "[/b]" & vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288) & Regex.Replace( _
    '                             exContent, "(<(/?p|br)[^<>]*>?|\\n|\\t)(\s|</?\w[^<>]*>?|\\n|\\t|\u00a0|\u3000)*", _
    '                             vbNewLine & vbNewLine & ChrW(12288) & ChrW(12288), RegexOptions.IgnoreCase).Trim & vbNewLine & vbNewLine)
    '        Next
    '        Return charRef.cleanRef(Regex.Replace(strExtras.ToString, "<((?<!>).)+", String.Empty).TrimEnd)
    '    Else
    '        Return String.Empty
    '    End If
    'End Function
#End Region

#Region "> ======== CHARACTER REFERENCE ========"
    Protected Class charRef
        Shared Function tightenWidth(ByVal Source As String) As String
            'convert from vbWide to vbNarrow
            Return Regex.Replace(Source, "[\uff00-\uff5e]", AddressOf toNarrowChar)
            'convert selected symbols to numeric code references
            'addRef = Regex.Replace(addRef, "[\u0080-\u00ff\u02b0-\u02ff\u2000-\u33ff\ufe30-\uffef]", AddressOf toNcrChar)
        End Function

        Private Shared Function toNarrowChar(ByVal m As Match) As Char
            'ff00-ff5e	65280-65374		Halfwidth and Fullwidth Forms (first-half)
            'IN ｀～！＠＃＄％＾＆＊（）［］｛｝＿｜＝＊－＋＜＞？，．；：＇＂
            'OUT  `~!@#$%^&*()[]{}_|=*-+<>?,.;:'"
            'tightenWidth = deprecated (｀|～|！|＠|＃|＄|％|＾|＆|＊|（|）|［|］|｛|｝|＿|｜|＝|＊|－|＋|＜|＞|？|，|．|；|：|＇|\uff02|／)
            Return StrConv(m.Value, vbNarrow, 2052)
        End Function

		'Deprecated
        'Private Shared Function toNcrChar(ByVal m As Match) As String
        '    '[0080-00ff][02b0-02ff]
        '    '0080-00ff		128-255			Extended ASCII
        '    '02b0-02ff		688-767			Spacing Modifier Letters

        '    'wide range     [2000-33ff]
        '    'narrow range   [2000-22ff][2460-25ff][2b00-2bff][2e80-303f][31c0-33ff]
        '    '2000-206f		8192-8303		General Punctuation
        '    '2070-209f		8304-8336		Superscripts and Subscripts / Super and Subscripts
        '    '20a0-20cf		8352-8399		Currency Symbols
        '    '2100-214f		8448-8527		Letterlike Symbols
        '    '2150-218f		8528-8591		Number Forms
        '    '2200-22ff		8704-8959		Mathematical Operators
        '    '2460-24ff		9312-9471		Enclosed Alphanumerics
        '    '2500-257f      9472-9599       Box Drawing
        '    '25a0-25ff      9632-9727       Geometric Shapes

        '    '2b00-2bff      11008-11263     Miscellaneous Symbols and Arrows
        '    '2e80-2eff		11904-12031		CJK Radicals Supplement
        '    '2f00-2fdf		12032-12255		CJK Radicals / KangXi Radicals
        '    '2ff0-2fff		12272-12287		Ideographic Description Characters
        '    '3000-303f		12288-12351		CJK Symbols and Punctuation
        '    '31c0-31ef		12736-12783		CJK Strokes
        '    '3200-32ff		12800-13055		Enclosed CJK Letters and Months
        '    '3300-33ff		13056-13311		CJK Compatibility

        '    '[fe30-ffef]
        '    'fe30-fe4f		65072-65103		CJK Compatibility Forms
        '    'fe50-fe6f		65104-65135		Small Form Variants
        '    '				65281-65374		Halfwidth and Fullwidth Forms (first-half)
        '    'ff5f-ffef		65375-65519		Halfwidth and Fullwidth Forms (second-half)

        '    'commom wide symbol to numeric code references base-10
        '    'new (　|、|。|《|》| |¦|¨|©|ª|«|®|°|±|²|³|µ|·|¹|»|¼|½|¾|×|÷|ˇ|ˉ|˜|‒|–|—|―|‖|‘|’|\u201C|\u201D|•|…|‰|‹|›|※|℃|℉|™|∞|∶|〈|〉|「|」|『|』|【|】|〔|〕|〖|〗|﹑|／|＼)
        '    'tightenWidth = old (　|、|。|《|》| |¦|¨|©|ª|«|®|°|±|²|³|µ|·|¹|»|¼|½|¾|×|÷|ˇ|ˉ|˜|‒|–|—|―|‖|‘|’|\u201C|\u201D|•|…|‰|‹|›|※|℃|℉|™|∞|∶|〈|〉|「|」|『|』|【|】|〔|〕|〖|〗|﹑|／|＼)
        '    Return "&#" & Convert.ToInt32(m.Value.ToCharArray.First) & ";"
        'End Function

        Shared Function cleanRef(ByVal Source As String) As String
            ''convert numeric character reference & character entity reference to character
            'numeric code references in Decimal (base-10)
            cleanRef = Regex.Replace(Source, "&#(?<ncr>\d+);", AddressOf ncrDec)
            'numeric code references in Hexadecimal (base-16)
            cleanRef = Regex.Replace(cleanRef, "&#x(?<ncr>[a-fA-F0-9]+);", AddressOf ncrHex)
            'character entity references
            cleanRef = Regex.Replace(cleanRef, "&(?<cer>\w+);", AddressOf ceRef)
            'remove html escape characters
            cleanRef = Regex.Replace(cleanRef, "\\[^\\]", AddressOf espChr)
            Return cleanRef
        End Function

        Private Shared Function ncrDec(ByVal m As Match) As Char
            Return Convert.ToChar(Integer.Parse(m.Groups("ncr").Value))
        End Function

        Private Shared Function ncrHex(ByVal m As Match) As Char
            Return Convert.ToChar(Convert.ToInt32(m.Groups("ncr").Value, 16))
        End Function

        Private Shared Function espChr(ByVal m As Match) As Char
            Return m.Value.Remove(0, 1)
        End Function

        Private Shared Function ceRef(ByVal m As Match) As Char
            Select Case m.Groups("cer").Value
                'common character entity references
                Case "quot" : Return """"
                Case "amp" : Return "&"
                Case "apos" : Return "'"
                Case "lt" : Return "<"
                Case "gt" : Return ">"
                Case "nbsp" : Return " "
                'rare character entity references
                Case "iexcl" : Return "¡"
                Case "cent" : Return "¢"
                Case "pound" : Return "£"
                Case "curren" : Return "¤"
                Case "yen" : Return "¥"
                Case "brvbar" : Return "¦"
                Case "sect" : Return "§"
                Case "uml" : Return "¨"
                Case "copy" : Return "©"
                Case "ordf" : Return "ª"
                Case "laquo" : Return "«"
                Case "not" : Return "¬"
                Case "shy" : Return ""
                Case "reg" : Return "®"
                Case "macr" : Return "¯"
                Case "deg" : Return "°"
                Case "plusmn" : Return "±"
                Case "sup2" : Return "²"
                Case "sup3" : Return "³"
                Case "acute" : Return "´"
                Case "micro" : Return "µ"
                Case "para" : Return "¶"
                Case "middot" : Return "·"
                Case "cedil" : Return "¸"
                Case "sup1" : Return "¹"
                Case "ordm" : Return "º"
                Case "raquo" : Return "»"
                Case "frac14" : Return "¼"
                Case "frac12" : Return "½"
                Case "frac34" : Return "¾"
                Case "iquest" : Return "¿"
                Case "Agrave" : Return "À"
                Case "Aacute" : Return "Á"
                Case "Acirc" : Return "Â"
                Case "Atilde" : Return "Ã"
                Case "Auml" : Return "Ä"
                Case "Aring" : Return "Å"
                Case "AElig" : Return "Æ"
                Case "Ccedil" : Return "Ç"
                Case "Egrave" : Return "È"
                Case "Eacute" : Return "É"
                Case "Ecirc" : Return "Ê"
                Case "Euml" : Return "Ë"
                Case "Igrave" : Return "Ì"
                Case "Iacute" : Return "Í"
                Case "Icirc" : Return "Î"
                Case "Iuml" : Return "Ï"
                Case "ETH" : Return "Ð"
                Case "Ntilde" : Return "Ñ"
                Case "Ograve" : Return "Ò"
                Case "Oacute" : Return "Ó"
                Case "Ocirc" : Return "Ô"
                Case "Otilde" : Return "Õ"
                Case "Ouml" : Return "Ö"
                Case "times" : Return "×"
                Case "Oslash" : Return "Ø"
                Case "Ugrave" : Return "Ù"
                Case "Uacute" : Return "Ú"
                Case "Ucirc" : Return "Û"
                Case "Uuml" : Return "Ü"
                Case "Yacute" : Return "Ý"
                Case "THORN" : Return "Þ"
                Case "szlig" : Return "ß"
                Case "agrave" : Return "à"
                Case "aacute" : Return "á"
                Case "acirc" : Return "â"
                Case "atilde" : Return "ã"
                Case "auml" : Return "ä"
                Case "aring" : Return "å"
                Case "aelig" : Return "æ"
                Case "ccedil" : Return "ç"
                Case "egrave" : Return "è"
                Case "eacute" : Return "é"
                Case "ecirc" : Return "ê"
                Case "euml" : Return "ë"
                Case "igrave" : Return "ì"
                Case "iacute" : Return "í"
                Case "icirc" : Return "î"
                Case "iuml" : Return "ï"
                Case "eth" : Return "ð"
                Case "ntilde" : Return "ñ"
                Case "ograve" : Return "ò"
                Case "oacute" : Return "ó"
                Case "ocirc" : Return "ô"
                Case "otilde" : Return "õ"
                Case "ouml" : Return "ö"
                Case "divide" : Return "÷"
                Case "oslash" : Return "ø"
                Case "ugrave" : Return "ù"
                Case "uacute" : Return "ú"
                Case "ucirc" : Return "û"
                Case "uuml" : Return "ü"
                Case "yacute" : Return "ý"
                Case "thorn" : Return "þ"
                Case "yuml" : Return "ÿ"
                Case "oelig" : Return "œ"
                Case "oelig" : Return "œ"
                Case "scaron" : Return "š"
                Case "scaron" : Return "š"
                Case "yuml" : Return "ÿ"
                Case "fnof" : Return "ƒ"
                Case "circ" : Return "ˆ"
                Case "tilde" : Return "˜"
                Case "Alpha" : Return "Α"
                Case "Beta" : Return "Β"
                Case "Gamma" : Return "Γ"
                Case "Delta" : Return "Δ"
                Case "Epsilon" : Return "Ε"
                Case "Zeta" : Return "Ζ"
                Case "Eta" : Return "Η"
                Case "Theta" : Return "Θ"
                Case "Iota" : Return "Ι"
                Case "Kappa" : Return "Κ"
                Case "Lambda" : Return "Λ"
                Case "Mu" : Return "Μ"
                Case "Nu" : Return "Ν"
                Case "Xi" : Return "Ξ"
                Case "Omicron" : Return "Ο"
                Case "Pi" : Return "Π"
                Case "Rho" : Return "Ρ"
                Case "Sigma" : Return "Σ"
                Case "Tau" : Return "Τ"
                Case "Upsilon" : Return "Υ"
                Case "Phi" : Return "Φ"
                Case "Chi" : Return "Χ"
                Case "Psi" : Return "Ψ"
                Case "Omega" : Return "Ω"
                Case "alpha" : Return "α"
                Case "beta" : Return "β"
                Case "gamma" : Return "γ"
                Case "delta" : Return "δ"
                Case "epsilon" : Return "ε"
                Case "zeta" : Return "ζ"
                Case "eta" : Return "η"
                Case "theta" : Return "θ"
                Case "iota" : Return "ι"
                Case "kappa" : Return "κ"
                Case "lambda" : Return "λ"
                Case "mu" : Return "μ"
                Case "nu" : Return "ν"
                Case "xi" : Return "ξ"
                Case "omicron" : Return "ο"
                Case "pi" : Return "π"
                Case "rho" : Return "ρ"
                Case "sigmaf" : Return "ς"
                Case "sigma" : Return "σ"
                Case "tau" : Return "τ"
                Case "upsilon" : Return "υ"
                Case "phi" : Return "φ"
                Case "chi" : Return "χ"
                Case "psi" : Return "ψ"
                Case "omega" : Return "ω"
                Case "thetasym" : Return "ϑ"
                Case "upsih" : Return "ϒ"
                Case "piv" : Return "ϖ"
                Case "ensp" : Return " "
                Case "emsp" : Return " "
                Case "zwnj" : Return ""
                Case "zwj" : Return ""
                Case "lrm" : Return ""
                Case "rlm" : Return ""
                Case "ndash" : Return "–"
                Case "mdash" : Return "—"
                Case "lsquo" : Return "‘"
                Case "rsquo" : Return "’"
                Case "sbquo" : Return "‚"
                Case "ldquo" : Return """"
                Case "rdquo" : Return """"
                Case "bdquo" : Return "„"
                Case "dagger" : Return "†"
                Case "Dagger" : Return "‡"
                Case "bull" : Return "•"
                Case "hellip" : Return "…"
                Case "permil" : Return "‰"
                Case "prime" : Return "′"
                Case "Prime" : Return "″"
                Case "lsaquo" : Return "‹"
                Case "rsaquo" : Return "›"
                Case "oline" : Return "‾"
                Case "frasl" : Return "⁄"
                Case "euro" : Return "€"
                Case "image" : Return "ℑ"
                Case "weierp" : Return "℘"
                Case "real" : Return "ℜ"
                Case "trade" : Return "™"
                Case "alefsym" : Return "ℵ"
                Case "larr" : Return "←"
                Case "uarr" : Return "↑"
                Case "rarr" : Return "→"
                Case "darr" : Return "↓"
                Case "harr" : Return "↔"
                Case "crarr" : Return "↵"
                Case "lArr" : Return "⇐"
                Case "uArr" : Return "⇑"
                Case "rArr" : Return "⇒"
                Case "dArr" : Return "⇓"
                Case "hArr" : Return "⇔"
                Case "forall" : Return "∀"
                Case "part" : Return "∂"
                Case "exist" : Return "∃"
                Case "empty" : Return "∅"
                Case "nabla" : Return "∇"
                Case "isin" : Return "∈"
                Case "notin" : Return "∉"
                Case "ni" : Return "∋"
                Case "prod" : Return "∏"
                Case "sum" : Return "∑"
                Case "minus" : Return "−"
                Case "lowast" : Return "∗"
                Case "radic" : Return "√"
                Case "prop" : Return "∝"
                Case "infin" : Return "∞"
                Case "ang" : Return "∠"
                Case "and" : Return "∧"
                Case "or" : Return "∨"
                Case "cap" : Return "∩"
                Case "cup" : Return "∪"
                Case "int" : Return "∫"
                Case "there4" : Return "∴"
                Case "sim" : Return "∼"
                Case "cong" : Return "≅"
                Case "asymp" : Return "≈"
                Case "ne" : Return "≠"
                Case "equiv" : Return "≡"
                Case "le" : Return "≤"
                Case "ge" : Return "≥"
                Case "sub" : Return "⊂"
                Case "sup" : Return "⊃"
                Case "nsub" : Return "⊄"
                Case "sube" : Return "⊆"
                Case "supe" : Return "⊇"
                Case "oplus" : Return "⊕"
                Case "otimes" : Return "⊗"
                Case "perp" : Return "⊥"
                Case "sdot" : Return "⋅"
                Case "lceil" : Return "⌈"
                Case "rceil" : Return "⌉"
                Case "lfloor" : Return "⌊"
                Case "rfloor" : Return "⌋"
                Case "lang" : Return "〈"
                Case "rang" : Return "〉"
                Case "loz" : Return "◊"
                Case "spades" : Return "♠"
                Case "clubs" : Return "♣"
                Case "hearts" : Return "♥"
                Case "diams" : Return "♦"
                Case Else : Return ""
            End Select
        End Function
    End Class
#End Region

End Class

