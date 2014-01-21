# MtimeScraper

MtimeScraper, .NET dynamic-link library for scraping movie information from Mtime.

The library written under concern of Movie, use against TV series will resulted as partially breaking.  
The is a PROOF OF CONCEPT on automate data retrieval from mtime.com  
It's fairly easy to integrated into other VB project but you shouldn't use it, especially as data grabbing tools.

What's more? It has a sister project, remember to look for [IMDbScraper](https://github.com/hebeguess/IMDbScraper) too.


### NOTICE
------

The library currently OUTDATED, my last commitment was near the end of 2011.  

At the time Mtime waged some hilarious crazy-style scramble words puzzle on their movie  
Plot & Extras pages to prevent data grabbing. (describe in next section)  
Obligatory login onto Mtime is necessity to gain acccess of information on both pages.  

These restrictions already waived by Mtime, the library hasn't been update since.  
The update will only involving some simple adjustments and clear out defuncted codes.  
I has no plan to continue contributing to this project currently, though.


#### [Deprecated] Anatomy of Mtime's data scrambling engine
------
Mtime implemented an undentified data scrambling engine on their movie  
Plot & Extras pages to protect their data.  
The only things I was able to recall were it was under MIT Lincense and supposely ASP.

Noting this data scrambling technique already received cease-of-use by Mtime.
I wish to detailing it as quite certain the same technique is still being implemented somewhere.
Hopefully, it will be useful to some of you who're now reading this.


A dynamic generated css full of those unique classes name is served when page load,  
containing classes are styled with *display:none* which designate to hide contents.

    // css snippet
    .m_s22{display:none;}.m_s30{display:none;}.m_s48{display:none;}.m_s55{display:none;}

The server will serves a mashed-up version of plot/extras data in JSON format through AJAX.  
Original paragraphs and letters concealed by garbage letters randomly interleaved between letters,  
then every single letter placed inside span element and tagged with an unique css class.

Congrats! payload just became over 20 times more bloat and messier.

    // plot snippet of json
    <span class=\"m_s46\">你</span><span class=\"m_s22\">踏</span><span class=\"m_s49\">好</span><span class=\"m_s55\">區</span>
    <span class=\"m_s48\">我</span><span class=\"m_s32\">?</span><span class=\"m_s21\">嗎</span><span class=\"m_s0\">?</span>


Each class name given to the garbage letter is no-coincidence but intended to match one class in the CSS.
From the magic of CSS, additional garbage letters eventually invisible across browsers.

    // display on the browsers
    你好嗎?

Albeit not visible, copying text on some browsers will reveal ALL the letters according to different layout engines.
  
    // resulf of text copying under gecko layout engine
    你踏好區我?嗎?

Complicated? No worries, all you need to do is supply correct information and the library will automatically solved it for you.


#### [Deprecated] Usage
-----
* Load and compiled the project in Visual Studio.
* Import the generated DLL into your desire projects.  

Before we begins, let's look at the shared aka. *static* functions first:

    // validate Mtime movie URL format
    bool b = Mtime.linkValidator(string url)

    // strip movie number from Mtime URL & validate URL
    string Movno = Mtime.getMovNo(string url)

    // login assistant; return cookies on login success, else empty string
    string cookies = Mtime.loginSpider(string userName, string passWord, Integer timeOut)
    
    // check if your cookies still valid; valid case 'Mtime Nick "yournick"',
    // if Mtime restart their server ^_^ 'Mtime User Not Login.'
    Mtime.loginChecker(string cookies, Integer timeOut)

    // grab and extract the css rules; return class name as string array
    string[] cssRules = Mtime.scScraper(string cookies, Integer timeOut)

***NOTE***: timeOut values are all optional (default=5000) because connections from China will be awful on occasions.

Declare new Mtime Class, pass either html source OR movie number from URL to constructors.

    // when you already have html content; pagesource, cssRules string array get from Mtime.scScraper()
    Mtime m = new Mtime(string pageSource, string[] cssRules);
	
    // library will handle contents loading for you; movie numbers from URL,
    // page number (describe in next section), cssRules string array get from Mtime.scScraper()
    Mtime m = new Mtime(Integer movNo, String pageNo, string[] cssRules, Integer timeOut);

Remember to apply threading if you're using the latter to avoid deadlock while waiting web response complete.



#### [Deprecated] Mtime's individual movie pages layout
------
Unlike IMDb you can grab most data on single webpage, Mtime seperated them into several pages


***Main movie page***

    string url = "http://movie.mtime.com/10014/"
    string pageNo = "1";
    Mtime m = new Mtime(_, pageNo, _);

    // other usages is simple, IDE will assists you along the way. Just try out yourself.
    string movieTitle = m.getTitle();
    
    FUNCTIONS: m.getTitleCn, m.getTitleHk, m.getTitleTw, m.getCompany, m.getCertificate,
               m.getCountry, m.getGenre, m.getPremiereDate, m.getPremiereCountry,
               m.getOfficialSite, m.getLanguage, m.getRuntime, m.getYear.


***Credit page***

    string url = "http://movie.mtime.com/10014/fullcredits.html"
    string pageNo = "2";
    Mtime m = new Mtime(_, pageNo, _);

    // check against specific field existent, sometimes Mtime left blank on certain fields
    bool b = m.checkDirector(); // Check if data exists
    b= m.checkCast();

    // retrieve data
    string s = m.getDirector();
    s = m.getCast();

	
***Plot page***

    string url = "http://movie.mtime.com/10014/plots.html"
    string pageNo = "3";
    Mtime m = new Mtime(_, pageNo, _);

    bool b = m.checkAvailability();
    string plot = m.getPlot();
    // other than actual plot, it may return string stated below:
    // "Not Available" indicating there are no plot data available for the movie
    // "Log-In First" need to have a valid cookies to access the data
    // "Fail" connection fails or etc.
    

***Extras page***

    string url = "http://movie.mtime.com/10014/behind__the__scene.html"
    string pageNo = "4";
    Mtime m = new Mtime(_, pageNo, _);

    bool b = m.checkAvailability()
    string extras = m.getExtras();
    // same as descriptions on plot section's getPlot()

Remember to apply threading if you're using the latter constructor to avoid deadlock while waiting web response complete.


### Terms of Use
------------

Mtime does not permit use of its data by third parties without their consent.

The project was motivated solely under personal purposed.

Please note you SHOULD NOT using this library from anything other than limited, personal and non-commercial use.

Neither I, nor any subsequent contributors of the project hold any kind of liability caused by your usage.

以上内容未经注明的，其版权均为Mtime所有。

未经Mtime书面许可，任何人不得以包括但不限于转载、引用、复制、镜像等形式使用以上内容。
