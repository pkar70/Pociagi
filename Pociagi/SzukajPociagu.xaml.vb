' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Public Class JedenEtap
    Public Property sStacja0 As String
    Public Property sStacja1 As String
    Public Property sData0 As String
    Public Property sData1 As String
    Public Property sArrival As String
    Public Property sDeparture As String
    Public Property sKurs As String ' nazwa pociagu, ktora moze sie zmieniac w trakcie
    Public Property sPeron0 As String    ' czasem nie ma tej kolumny!
    Public Property sPeron1 As String    ' czasem nie ma tej kolumny!
    Public Property sLink As String
    Public Property sInfo As String
End Class

Public Class JednoPolaczenie
    Public Property sFrom As String
    Public Property sTo As String
    Public Property sData As String
    Public Property sDepart As String
    Public Property sArrival As String
    Public Property sCzas As String
    Public Property sPrzesiadki As String
    Public Property sCzym As String
    Public Property sId As String
    Public Property sDetails As String
    Public Property oEtapy As Collection(Of JedenEtap)
    Public Property sAddInfo As String
End Class


''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class SzukajPociagu
    Inherits Page

    Private msStart As String
    Private msEnd As String

    Private msLink As String

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiData.Date = Date.Now
        ' CalendarPicker
        uiData.MaxDate = New DateTimeOffset(Date.Now.Year + 1, 12, 31, 0, 0, 0, New TimeSpan(0))
        uiData.MinDate = Date.Now.AddDays(-1)
        ' DatePicker
        'uiData.MaxYear = New DateTimeOffset(Date.Now.Year + 1, 12, 31, 0, 0, 0, New TimeSpan(0))
        'uiData.MinYear = New DateTimeOffset(Date.Now.Year - 1, 1, 1, 0, 0, 0, New TimeSpan(0))

        uiCzas.Time = New TimeSpan(Date.Now.Hour, Date.Now.Minute, 0)

        Await App.InitPunkty(False)
        App.moPunkty.MarkShow("")       ' znaczy tylko favourites beda
        uiStartCombo.ItemsSource = App.moPunkty.GetSourceNames
        uiEndCombo.ItemsSource = App.moPunkty.GetSourceNames
        msStart = ""
        msEnd = ""

        FormatkaSzukaniaEnable(App.moPolaczenia Is Nothing)
    End Sub

    Private Async Function PoszukajPociaguDesktop(bPrzyjaz As Boolean) As Task
        Dim sHost As String = "http://rozklad-pkp.pl/pl/tp?"

        Dim sDate As String = uiData.Date.ToString("dd.MM.yy")
        Dim sTime As String = uiCzas.Time.ToString("hh\%\3\Amm")
        'Dim sUri As String = sHost &
        '    "REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0G=" & uiStart.Text &
        '    "&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0G=" & uiEnd.Text &
        '    "&date=" & sDate & "&time=" & sTime

        'Dim sPage As String
        'sPage = Await App.HttpPageAsync(sUri, "search page init")
        'If sPage = "" Then Exit Function
        ' http://rozklad-pkp.pl/pl/tp?queryPageDisplayed=yes&REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0G=5100028&REQ0JourneyStopsS0ID=&REQ0JourneyStops1.0G=&REQ0JourneyStopover1=&REQ0JourneyStops2.0G=&REQ0JourneyStopover2=&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0G=5100065&REQ0JourneyStopsZ0ID=&date=13.08.18&dateStart=13.08.18&dateEnd=13.08.18&REQ0JourneyDate=13.08.18&time=14%3A10&REQ0JourneyTime=14%3A10&REQ0HafasSearchForw=1&existBikeEverywhere=yes&existHafasAttrInc=yes&existHafasAttrInc=yes&REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_1_0=1&REQ0JourneyProduct_prod_section_2_0=1&REQ0JourneyProduct_prod_section_3_0=1&REQ0JourneyProduct_prod_section_0_1=1&REQ0JourneyProduct_prod_section_1_1=1&REQ0JourneyProduct_prod_section_2_1=1&REQ0JourneyProduct_prod_section_3_1=1&REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_1_2=1&REQ0JourneyProduct_prod_section_2_2=1&REQ0JourneyProduct_prod_section_3_2=1&REQ0JourneyProduct_prod_section_0_3=1&REQ0JourneyProduct_prod_section_1_3=1&REQ0JourneyProduct_prod_section_2_3=1&REQ0JourneyProduct_prod_section_3_3=1&REQ0JourneyProduct_opt_section_0_list=0%3A000000&existOptimizePrice=1&existHafasAttrExc=yes&REQ0HafasChangeTime=0%3A1&existSkipLongChanges=0&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&REQ0HafasAttrExc=&existHafasAttrInc=yes&existHafasAttrExc=yes&wDayExt0=Pn%7CWt%7C%C5%9Ar%7CCz%7CPt%7CSo%7CNd&start=start&existUnsharpSearch=yes&came_from_form=1#focus
        Dim iStart As Integer = Await App.moPunkty.GetPKPIdAsync(uiStart.Text, True)
        Dim iStop As Integer = Await App.moPunkty.GetPKPIdAsync(uiEnd.Text, True)

        If iStart = 0 OrElse iStop = 0 Then
            App.DialogBox("Nie moge znalezc identyfikatora stacji")
            Exit Function
        End If

        Dim sUri As String = sHost & "queryPageDisplayed=yes&REQ0JourneyProduct_prod_section_0_1=1&start=start&" &
            "REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0G=51" & iStart.ToString("00000") &
            "&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0G=51" & iStop.ToString("00000") &
            "&date=" & sDate & "&time=" & sTime

        Dim sPage As String
        sPage = Await App.HttpPageAsync(sUri, "search page")
        If sPage = "" Then Exit Function

        ' Zakopane: 5196013
        ' Kielce: 5100022
        ' REQ0JourneyStopsS0A: 1
        ' REQ0JourneyStopsS0G: 5196013
        ' REQ0JourneyStopsZ0A: 1
        ' REQ0JourneyStopsZ0G: 5100022
        ' date: 10.08.18
        ' time: 
    End Function

    Private Function ExtractStopSymbol(sPage As String, sTyp As String, sName As String)
        Dim iInd As Integer
        Dim sRet As String

        iInd = sPage.IndexOf(sTyp)
        iInd = sPage.LastIndexOf("<", iInd)

        If sPage.Substring(iInd, 6) = "<input" Then
            ' to jest jednoznaczne
            ' <input type="hidden" name="REQ0JourneyStopsS0K" value="S-0N1">
            iInd = sPage.IndexOf("value", iInd)
            sRet = sPage.Substring(iInd + 7, 15)
            iInd = sRet.IndexOf("""")
            Return sRet.Substring(0, iInd)
        End If

        ' teraz trzeba znalezc
        '<select id = "to"  name="REQ0JourneyStopsZ0K" title="">
        '<option value = "S-1N1" > KRAK&#211;W-     </Option>
        '<option value = "S-1N2" > Krak&#243;w G&#322;&#243;wny     </Option>
        If sPage.Substring(iInd, 7) <> "<select" Then
            App.DialogBox("ERROR: nieznany typ odpowiedzi na szukanie")
            Return ""
        End If

        sRet = sPage.Substring(iInd)
        iInd = sRet.IndexOf("</select")
        sRet = sRet.Substring(0, iInd)
        sRet = Net.WebUtility.HtmlDecode(sRet).ToLower
        sName = ">" & sName.ToLower & "<"
        iInd = sRet.IndexOf(sName)
        If iInd < 1 Then
            App.DialogBox("ERROR: nie moge znalezc stacji?")
            Return ""
        End If
        iInd = sRet.LastIndexOf("""", iInd)
        sRet = sRet.Substring(0, iInd)
        iInd = sRet.LastIndexOf("""")
        sRet = sRet.Substring(iInd)

        Return sRet
    End Function

    Private Async Function GetStronaPolaczenMobile(oData As DateTimeOffset, oTime As TimeSpan, sStart As String, sEnd As String) As Task(Of String)
        Dim sHost As String = "http://mobil.rozklad-pkp.pl/bin/query.exe/pn?"

        Dim sDate As String = oData.ToString("dd.MM.yy")
        Dim sTime As String = oTime.ToString("hh\%\3\Amm")

        Dim sUri As String = sHost &
            "queryPageDisplayed=yes&REQ0HafasSearchForw=1&existUnsharpSearch=yes&start=Wyszukaj" &
            "REQ0JourneyProduct_opt_section_0_list=0%3A000000&" &
            "REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_0_1=1&" &
            "REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_0_3=1&" &
            "REQ0JourneyDate=" & sDate & "&REQ0JourneyTime=" & sTime & "&" &
            "REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0G=" & Uri.EscapeUriString(sStart) & "&" &
            "REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0G=" & Uri.EscapeUriString(sEnd)

        Dim sPage As String
        sPage = Await App.HttpPageAsync(sUri, "search page")
        If sPage = "" Then Return ""

        Dim iInd As Integer
        iInd = sPage.IndexOf("Wprowadzone dane nie s") ' "ą jednoznaczne", ale jako s&#261;
        If iInd > 0 Then
            Dim sStart1 As String = ExtractStopSymbol(sPage, "REQ0JourneyStopsS0K", sStart)
            If sStart1 = "" Then Return ""
            Dim sStop As String = ExtractStopSymbol(sPage, "REQ0JourneyStopsZ0K", sEnd)
            If sStop = "" Then Return ""

            iInd = sPage.IndexOf("<form")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("action")
            sPage = sPage.Substring(iInd + 8)
            iInd = sPage.IndexOf("""")
            sUri = sPage.Substring(0, iInd)

            sUri = sUri & "&REQ0JourneyDate=" & sDate & "&REQ0JourneyTime=" & sTime
            sUri = sUri & "&REQ0JourneyProduct_opt_section_0_list=0%3A000000"
            sUri = sUri & "&REQ0JourneyStopsS0K=" & sStart
            sUri = sUri & "&REQ0JourneyStopsZ0K=" & sStop

            sPage = Await App.HttpPageAsync(sUri, "search page2")
            If sPage = "" Then Return ""

        End If

        Return sPage
    End Function

    Private Function ExtractStronaPolaczenMobile(sPage As String) As String
        Dim iInd As Integer
        Dim sLink As String

        ' extract linku formatki do szczegółów
        ' <form name = "tp_results_form" action="http://mobil.rozklad-pkp.pl/bin/query.exe/pn?ld=mobil&amp;seqnr=4&amp;ident=7b.0201740.1534443454&amp;OK#focus" method="post" style="display:inline">
        iInd = sPage.IndexOf("<form")
        If iInd < 1 Then Return ""

        sPage = sPage.Substring(iInd)
        iInd = sPage.IndexOf("action=")
        sPage = sPage.Substring(iInd + 8)
        iInd = sPage.IndexOf("""")
        sLink = sPage.Substring(0, iInd)
        iInd = sLink.IndexOf("#")
        If iInd > 0 Then sLink = sLink.Substring(0, iInd)   ' bez #focus - bo to ucina link :)


        ' a teraz wyniki (znalezione pociagi)
        ' link do wczesniej:
        ' <a href="http://mobil.rozklad-pkp.pl/bin/query.exe/pn?ld=mobil&amp;seqnr=4&amp;ident=7b.0201740.1534443454&amp;REQ0HafasScrollDir=2" accesskey="e"><img src="/hafas-res/img/ok/wczesniej.gif" border="0" alt="wcze&#347;niej"></a>
        ' link do pozniej
        ' <a href="http://mobil.rozklad-pkp.pl/bin/query.exe/pn?ld=mobil&amp;seqnr=4&amp;ident=7b.0201740.1534443454&amp;REQ0HafasScrollDir=1" accesskey="l"><img src="/hafas-res/img/ok/pozniej.gif" border="0" alt="p&#243;&#378;niej"></a>

        ' wycinamy tylko rzadki z połączeniami
        iInd = sPage.IndexOf("REQ0HafasScrollDir")
        sPage = sPage.Substring(iInd + 1)
        iInd = sPage.IndexOf("REQ0HafasScrollDir")
        iInd = sPage.LastIndexOf("<tr", iInd)
        sPage = sPage.Substring(0, iInd)

        App.moPolaczenia = New Collection(Of JednoPolaczenie)

        Dim bEmpty = True
        iInd = sPage.IndexOf("<tr")
        While iInd > 0
            bEmpty = False
            sPage = sPage.Substring(iInd + 1)

            Dim oItem As JednoPolaczenie = New JednoPolaczenie

            iInd = sPage.IndexOf("<td")             ' kolumna z checkbox do szczegolow
            '<td headers="hafasOVCheckbox" Class="sepline center nowrap">
            '<span style="display:none"><label For="ovCheckbox0">Numer po&#322;&#261;czenia 0</label></span>
            '<input id="ovCheckbox0" type="checkbox" name="guiVCtrl_connection_detailsOut_select_C1-0"  value="yes">
            '</td>

            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("<td")   ' stacja wyjazd/przyjazd
            oItem.sId = sPage.Substring(0, iInd)
            sPage = sPage.Substring(iInd)

            iInd = oItem.sId.IndexOf("guiVCtrl_")
            oItem.sId = oItem.sId.Substring(iInd)
            iInd = oItem.sId.IndexOf("""")
            oItem.sId = oItem.sId.Substring(0, iInd)

            iInd = sPage.IndexOf("<br")     ' pomiedzy wyjazd/przyjazd
            oItem.sFrom = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("<td", 1)     ' do kolumny data
            oItem.sTo = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("<td", 1)     ' do kolumny z legendą odj/przyj
            oItem.sData = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim & ":"
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("<td", 1)     ' do kolumny z godzinami
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("<br")     ' pomiedzy wyjazd/przyjazd
            oItem.sDepart = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim & "→"
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("<td")     ' do kolumny planowy/opoznienie
            oItem.sArrival = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("<td", iInd + 2)     ' do kolumny czas przejazdu
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("<td", 1)     ' do kolumny z przesiadkami
            oItem.sCzas = "(" & Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim & ")"
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("<td", 1)     ' do kolumny z kursami
            oItem.sPrzesiadki = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sPage.Substring(0, iInd))).Trim
            If oItem.sPrzesiadki = "0" Then oItem.sPrzesiadki = ""
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("</td")

            Dim sTxt As String = sPage.Substring(0, iInd)
            '<td headers="hafasOVProducts" Class="sepline screennowrap">
            '<img src="/hafas-res/img/kml_pic.gif" width="28" height="30" alt="KML33429">
            '<img src="/hafas-res/img/reg_pic.gif" width="28" height="30" alt="R  30511">
            '</td>
            iInd = sTxt.IndexOf("/img/")
            While iInd > 0
                sTxt = sTxt.Substring(iInd + 5)
                oItem.sCzym = oItem.sCzym & sTxt.Substring(0, 3).ToUpper & " "
                iInd = sTxt.IndexOf("/img/")
            End While

            'Public Property sCzym As String
            App.moPolaczenia.Add(oItem)

            iInd = sPage.IndexOf("<tr")
        End While

        If bEmpty Then App.DialogBox("Brak połączeń?")

        Return sLink

    End Function

    Private Async Function GetStronaDetailsow() As Task(Of String)
        ' http://mobil.rozklad-pkp.pl/bin/query.exe/pn?ld=mobil&seqnr=1&ident=4r.0468740.1534587087&OK
        '    guiVCtrl_connection_detailsOut_select_C0-0: yes
        '    jumpToDetails%3Dyes%26guiVCtrl_connection_detailsOut_add_group_overviewOut: Poka%C5%BC+szczeg%C3%B3%C5%82owo+wszystkie+po%C5%82%C4%85czenia
        '    sortConnections: unsorted
        '    test0: x
        Dim sUri As String ' = Uri.UnescapeDataString(msLink) ' & "&jumpToDetails%3Dyes%26guiVCtrl_connection_detailsOut_add_group_overviewOut: Poka"
        sUri = Net.WebUtility.HtmlDecode(msLink) & "&jumpToDetails=yes&guiVCtrl_connection_detailsOut_add_group_overviewOut=Poka"
        Dim sPage As String = Await App.HttpPageAsync(sUri, "szczegoly polaczen")
        Return sPage
    End Function
    Private Sub ExtractDetailsPolaczen(sPage As String)
        If sPage = "" Then Exit Sub

        Dim iInd As Integer
        Dim sEntry As String
        Dim iGuard = 10

        While iGuard > 0
            iGuard -= 1
            iInd = sPage.IndexOf("Widok szczeg")
            If iInd < 0 Then Exit While
            iInd = sPage.IndexOf("<tr", iInd)
            sPage = sPage.Substring(iInd)

            iInd = sPage.IndexOf("Widok szczeg")
            If iInd > 0 Then
                sEntry = sPage.Substring(0, iInd)
            Else
                sEntry = sPage
            End If

            iInd = sEntry.IndexOf("_TimeDep")        ' rzadek naglowka, hafasDTL0_TimeDep, potem hafasDTL1_TimeDep, etc.
            iInd = sEntry.IndexOf("_TimeDep", iInd + 5) ' rzadek danych pierwszego
            iInd = sEntry.IndexOf(">", iInd)

            Dim bFound = False
            For Each oItem As JednoPolaczenie In App.moPolaczenia
                If oItem.sDepart = sEntry.Substring(iInd + 1, 5) & "→" Then
                    oItem.sDetails = Net.WebUtility.HtmlDecode(sEntry)
                    ConvertDetails2Etapy(oItem)
                    bFound = True
                    Exit For
                End If
            Next
            If Not bFound Then
                App.DialogBox("Nie moge znalezc polaczenia dla szczegolow")
            End If


        End While

        'Znajdz: "czenia - widok szczeg"
        '<td z hafasDTL0_Stop - nazwa
        '<td z hafasDTL0_Date
        '<td z hafasDTL0_TimeArr 
        '<td z hafasDTL0_TimeDep (<td headers="hafasDTL0_TimeDep" class="center sepline1">03:06<br><span class="prognosis"></span></td>)
        '<td z hafasDTL0_Platform
        '<td z hafasDTL0_Products , eic_pic.gif , alt="nazwa"
        '<td z hafasDTL0_Remarks (rowspan=2) - dalsze uwagi

        'potem od "Czas trwania" do "<" jest info

    End Sub

    Private Sub ConvertDetails2Etapy(ByRef oItem As JednoPolaczenie)
        ' oItem.sDetails jest do konwersji do oItem.oEtapy i oItem.sAddInfo
        Dim sTxt As String = oItem.sDetails
        Dim iInd As Integer

        oItem.oEtapy = New Collection(Of JedenEtap)

        iInd = sTxt.IndexOf("stboard.exe")
        While iInd > 1
            Dim oEtap As JedenEtap = New JedenEtap
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sStacja0 = sTxt.Substring(0, iInd)

            iInd = sTxt.IndexOf("_Date")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sData0 = App.RemoveHtmlTags(sTxt.Substring(0, iInd)).Trim

            iInd = sTxt.IndexOf("_TimeDep")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("</td")
            oEtap.sDeparture = App.RemoveHtmlTags(sTxt.Substring(0, iInd))

            iInd = sTxt.IndexOf("_Platform")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sPeron0 = App.RemoveHtmlTags(sTxt.Substring(0, iInd)).Trim

            iInd = sTxt.IndexOf("alt=")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf("""", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("""")
            oEtap.sKurs = App.RemoveHtmlTags(sTxt.Substring(0, iInd))

            iInd = sTxt.IndexOf("traininfo.exe")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.LastIndexOf("""", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("""")
            oEtap.sLink = sTxt.Substring(0, iInd)

            iInd = sTxt.IndexOf("_Remarks")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("</td")
            oEtap.sInfo = App.RemoveHtmlTags(sTxt.Substring(0, iInd))
            If oEtap.sInfo <> "" Then
                oEtap.sInfo = vbCrLf & oEtap.sInfo & vbCrLf     ' uładnienie wyglądu
            End If


            iInd = sTxt.IndexOf("stboard.exe")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sStacja1 = sTxt.Substring(0, iInd)

            iInd = sTxt.IndexOf("_Date")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sData1 = App.RemoveHtmlTags(sTxt.Substring(0, iInd)).Trim

            iInd = sTxt.IndexOf("_TimeArr")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("</td")
            oEtap.sArrival = App.RemoveHtmlTags(sTxt.Substring(0, iInd))

            iInd = sTxt.IndexOf("_Platform")
            If iInd < 1 Then Exit Sub
            iInd = sTxt.IndexOf(">", iInd)
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("<")
            oEtap.sPeron1 = App.RemoveHtmlTags(sTxt.Substring(0, iInd)).Trim


            oItem.oEtapy.Add(oEtap)

            iInd = sTxt.IndexOf("stboard.exe")
            If iInd > sTxt.IndexOf("Czas trwania") Then iInd = 0
        End While

        iInd = sTxt.IndexOf("Czas trwania")
        sTxt = sTxt.Substring(iInd)
        iInd = sTxt.IndexOf("</td", iInd)
        oItem.sAddInfo = App.RemoveHtmlTags(sTxt.Substring(0, iInd))

    End Sub

    Private Async Function PoszukajPociaguMobile(bPrzyjaz As Boolean) As Task
        Dim sPage As String = Await GetStronaPolaczenMobile(uiData.Date, uiCzas.Time, uiStart.Text, uiEnd.Text)
        If sPage = "" Then Exit Function

        msLink = ExtractStronaPolaczenMobile(sPage)
        If msLink = "" Then Exit Function
        If App.moPolaczenia.Count < 1 Then
            msLink = ""
            Exit Function
        End If

        FormatkaSzukaniaEnable(False) ' skasuj formatke szukania
        uiListItems.ItemsSource = App.moPolaczenia

        ExtractDetailsPolaczen(sPage) ' tym razem bezdie tylko jedno

        sPage = Await GetStronaDetailsow()
        If sPage = "" Then Exit Function
        ExtractDetailsPolaczen(sPage) ' a tym razem wszystkie

    End Function


    Private Sub FormatkaSzukaniaEnable(bShow As Boolean)
        Dim bVis As Visibility = If(bShow, Visibility.Visible, Visibility.Collapsed)
        uiGridRow2.Visibility = bVis
        uiStartCombo.Visibility = bVis
        uiEndCombo.Visibility = bVis
        uiStart.IsReadOnly = Not bShow  ' rozne, bo sprawdzam co lepsze
        uiEnd.IsReadOnly = Not bShow
        If bShow Then
            uiStart.Header = "Wyjazd z"
            uiEnd.Header = "Dokąd"
        Else
            uiStart.Header = ""
            uiEnd.Header = ""
        End If
        ' uiEnd.IsEnabled = bShow
    End Sub

    Private Async Sub uiSearchOdjazdy_Click(sender As Object, e As RoutedEventArgs)
        Await PoszukajPociaguMobile(False)
    End Sub

    Private Async Sub uiSearchPrzyjazdy_Click(sender As Object, e As RoutedEventArgs)
        Await PoszukajPociaguMobile(True)
    End Sub



    Dim mbInCombo As Boolean = False

    Private Async Sub uiStart_TextChanged(sender As Object, e As TextChangedEventArgs) Handles uiStart.TextChanged
        If mbInCombo Then Exit Sub   ' zeby nie bylo wzajemnego wywolywania przy zmianie combo/edit

        If App.GetSettingsBool("nazwyOnline") Then
            uiStartCombo.ItemsSource = Await App.GetOnlineStationNameAsync(uiStart.Text, True)
        Else
            If uiStart.Text.Length < 3 Then
                App.moPunkty.MarkShow("")
            Else
                App.moPunkty.MarkShow(uiStart.Text)
            End If
            uiStartCombo.ItemsSource = App.moPunkty.GetSourceNames
        End If
    End Sub

    Private Sub uiStartCombo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles uiStartCombo.SelectionChanged
        mbInCombo = True
        Dim sTmp As String = uiStartCombo.SelectedValue
        If sTmp IsNot Nothing Then uiStart.Text = sTmp

        mbInCombo = False
    End Sub

    Private Async Sub uiEnd_TextChanged(sender As Object, e As TextChangedEventArgs) Handles uiEnd.TextChanged
        If mbInCombo Then Exit Sub   ' zeby nie bylo wzajemnego wywolywania przy zmianie combo/edit

        If App.GetSettingsBool("nazwyOnline") Then
            uiEndCombo.ItemsSource = Await App.GetOnlineStationNameAsync(uiEnd.Text, True)
        Else
            If uiEnd.Text.Length < 3 Then
                App.moPunkty.MarkShow("")
            Else
                App.moPunkty.MarkShow(uiEnd.Text)
            End If
            uiEndCombo.ItemsSource = App.moPunkty.GetSourceNames
        End If
    End Sub

    Private Sub uiEndCombo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles uiEndCombo.SelectionChanged
        mbInCombo = True
        Dim sTmp As String = uiEndCombo.SelectedValue
        If sTmp IsNot Nothing Then uiEnd.Text = sTmp

        mbInCombo = False
    End Sub

    Private Sub uiShowDaty_Click(sender As Object, e As RoutedEventArgs)
        FormatkaSzukaniaEnable(True)
    End Sub

    Private Sub uiDetails_Click(sender As Object, e As RoutedEventArgs)
        ' pokazanie (we flyout?) szczegolow połączenia
        Dim oItem As JednoPolaczenie
        oItem = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, JednoPolaczenie)

        uiDetailsItems.ItemsSource = oItem.oEtapy
        uiFlyoutDetails.ShowAt(uiTitle)

    End Sub

    Private Sub uiMakeFav_Click(sender As Object, e As RoutedEventArgs)
        ' z sDetails wyciagniecie linku do pociagu, i ustawienie go domyslnym
        ' ale to nie do konca tak, bo moze byc podroz z przesiadkami
        Dim oItem As JednoPolaczenie
        oItem = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, JednoPolaczenie)

        If oItem.sPrzesiadki <> "" Then App.DialogBox("Połączenie z przesiadkami, użyję tylko pierwszego fragmentu")

        Dim sTxt = oItem.sDetails
        Dim iInd As Integer
        iInd = sTxt.IndexOf("traininfo.exe")
        If iInd < 1 Then Exit Sub
        iInd = sTxt.LastIndexOf("""", iInd)
        sTxt = sTxt.Substring(iInd + 1)
        iInd = sTxt.IndexOf("""")
        sTxt = sTxt.Substring(0, iInd)
        sTxt = sTxt.Replace("&amp;", "&")

        App.SetSettingsString("CurrTrainLink", sTxt)
        App.SetSettingsString("CurrTrainName", oItem.sFrom & " - " & oItem.sTo)
        App.SetSettingsInt("CurrTrainSetTime", Date.Now.DayOfYear)

    End Sub

    Private Sub uiMakeFavSubTrain_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As JedenEtap
        oItem = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, JedenEtap)
        App.SetSettingsString("CurrTrainLink", oItem.sLink)
        App.SetSettingsString("CurrTrainName", oItem.sStacja0 & " - " & oItem.sStacja1)
        App.SetSettingsInt("CurrTrainSetTime", Date.Now.DayOfYear)

    End Sub

    ' szczegoly połączenia
    '             guiVCtrl_connection_detailsOut_select_C0-1: yes
    ' jumpToDetails%3Dyes%26guiVCtrl_connection_detailsOut_add_selection: Poka%C5%BC+szczeg%C3%B3%C5%82owo+wybrane+po%C5%82%C4%85czenia
    ' sortConnections: unsorted
    'test0:      x
    ' http://mobil.rozklad-pkp.pl/bin/query.exe/pn?ld=mobil&seqnr=1&ident=ev.02975640.1534498540&OK
    ' daje to perony, info o np. 'tylko druga klasa', oraz link do trasy pociagu
    ' w kazdym <tr jest wtedy pearl_start_soc.gif , pearl_middle_poc.gif , pearl_middle_eoc.gif na trasie, i pearl_middle.gif oraz pearl_end.gif  pozniej
    ' link do pociagu: http://mobil.rozklad-pkp.pl/bin/traininfo.exe/pn/186669/251790/848230/361892/51?ld=mobil&seqnr=1&ident=g0.02791440.1534502306&date=17.08.18&station_evaId=5100240&station_type=dep&journeyStartIdx=0&journeyEndIdx=23&backLink=tp&

End Class
