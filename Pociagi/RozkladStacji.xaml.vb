' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Public Class EntryRozkladu
    Public Property sTime As String
    Public Property sDate As String
    Public Property sKurs As String
    Public Property sLinkKurs As String
    Public Property sDestination As String
    Public Property sPrzez As String
    Public Property sPeron As String
End Class

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
'''
Public NotInheritable Class RozkladStacji
    Inherits Page

    ' default: data: dziś
    ' zmiana daty z "..."

    ' zapewnienie przezywalnosci pomiedzy stronami
    'Private Shared mlPociagiStacji As Collection(Of EntryRozkladu) = Nothing
    Private msNazwa As String ' parametr wejsciowy do formatki - nazwa stacji której pokazać rozkład
    Private moDate As DateTime
    Private mbLastType As Boolean

    Private mbInsideInit As Boolean = False

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)
        msNazwa = e.Parameter.ToString
        If App.msStacjaNazwa <> msNazwa Then
            App.msStacjaNazwa = msNazwa
            App.moStacjaDate = Date.Now.AddMinutes(-10)    ' od 10 minut temu
            App.mlPociagiStacji = Nothing
            App.mbStacjaLastType = False
        End If

        mbLastType = App.mbStacjaLastType
        moDate = App.moStacjaDate
        msNazwa = App.msStacjaNazwa
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        mbInsideInit = True
        uiTitle.Text = msNazwa
        uiKiedy.Visibility = Visibility.Collapsed
        uiData.Date = moDate
        uiData.MaxYear = New DateTimeOffset(moDate.Year + 1, 12, 31, 0, 0, 0, New TimeSpan(0))
        uiData.MinYear = New DateTimeOffset(moDate.Year - 1, 1, 1, 0, 0, 0, New TimeSpan(0))
        uiCzas.Time = New TimeSpan(moDate.Hour, moDate.Minute, 0)
        uiShowOdjazdy_Click(Nothing, Nothing)
        mbInsideInit = False
    End Sub

    Private Sub uiShowDaty_Click(sender As Object, e As RoutedEventArgs)
        If uiKiedy.Visibility = Visibility.Visible Then
            uiKiedy.Visibility = Visibility.Collapsed
        Else
            uiKiedy.Visibility = Visibility.Visible
        End If
    End Sub

    Private Async Function CreateUriDesktop(bPrzyjazdy As Boolean) As Task(Of String)
        Dim sData As String = uiData.Date.ToString("dd.MM.yy")
        Dim sUri1 As String = "http://rozklad-pkp.pl/pl/sq" &
                "?maxJourneys=10" &
                "&input=" & Uri.EscapeUriString(msNazwa) &
                "&REQStationS0F=&excludeStationAttribute%3BM-&disableEquivs=yes" &
                "&date=" & sData & "&dateStart=" & sData & "&REQ0JourneyDate=" & sData &
                "&dateEnd=" & sData &
                "&time=" & uiCzas.Time.ToString("hh\%\3\Amm")
        ' "&dateEnd=" & uiData.Date.AddDays(+10).ToString("dd.MM.yy") &

        Dim sStation As String = "51" & (Await App.moPunkty.GetPKPIdAsync(msNazwa, True)).ToString("00000")
        Dim sUri As String = "http://rozklad-pkp.pl/pl/sq" &
                "?maxJourneys=10" &
                "&input=" & sStation &
                "&REQStationS0F=&excludeStationAttribute%3BM-&disableEquivs=yes" &
                "&date=" & sData & "&dateStart=" & sData & "&REQ0JourneyDate=" & sData &
                "&dateEnd=" & sData &
                "&time=" & uiCzas.Time.ToString("hh\%\3\Amm")

        If bPrzyjazdy Then
            sUri &= "&boardType=arr"
            sUri1 &= "&boardType=arr"
        Else
            sUri &= "&boardType=dep"
            sUri1 &= "&boardType=dep"
        End If

        sUri &= "&GUIREQProduct_0=on&GUIREQProduct_1=on&GUIREQProduct_2=on&GUIREQProduct_3=on&advancedProductMode=&start=#focus"
        sUri1 &= "&GUIREQProduct_0=on&GUIREQProduct_1=on&GUIREQProduct_2=on&GUIREQProduct_3=on&advancedProductMode=&start=#focus"

        ' sUri = "http://rozklad-pkp.pl/pl/sq?maxJourneys=10&input=5100240&REQStationS0F=&disableEquivs=yes&date=13.08.18&dateStart=13.08.18&REQ0JourneyDate=13.08.18&time=21%3A29&boardType=dep&GUIREQProduct_0=on&GUIREQProduct_1=on&GUIREQProduct_2=on&GUIREQProduct_3=on&maxJourneys=10&dateEnd=13.08.18&advancedProductMode=&start=#focus"
        Return sUri
    End Function
    Private Sub ImportTablicaStacjiDesktop(sPage As String)
        Dim sLastDate As String = uiData.Date.ToString("dd")

        Dim iInd As Integer
        iInd = sPage.IndexOf("<tr class")
        While iInd > 0
            Dim oNew = New EntryRozkladu

            sPage = sPage.Substring(iInd + 5)

            '<span Class="time">23:32</span><input type="hidden" value="03.08.18">
            iInd = sPage.IndexOf("time")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf(">")
            sPage = sPage.Substring(iInd + 1)

            oNew.sTime = sPage.Substring(0, 5)

            ' jesli data jest nie tego dnia... niekoniecznie to następny dzień!
            iInd = sPage.IndexOf("value")
            sPage = sPage.Substring(iInd + 7)
            If sPage.Substring(0, 2) <> sLastDate Then
                oNew.sDate = "(" & sPage.Substring(0, 5) & ")"
                sLastDate = sPage.Substring(0, 2)
            Else
                oNew.sDate = ""
            End If

            ' a href="XX" jako .sLinkKurs
            iInd = sPage.IndexOf("href")
            sPage = sPage.Substring(iInd + 6)
            iInd = sPage.IndexOf("""")
            oNew.sLinkKurs = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd)) ' htmldecode, bo jest za duzo &amp; :)

            ' alt="XX" jako .sKurs
            '(choć może lepsze by było nie stąd a z 
            ' <td 
            ' </a , wstecz do > , -> .sKurs
            iInd = sPage.IndexOf("alt")
            sPage = sPage.Substring(iInd + 5)
            iInd = sPage.IndexOf("""")
            oNew.sKurs = sPage.Substring(0, iInd)

            iInd = sPage.IndexOf("<td")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("</a")
            iInd = sPage.LastIndexOf(">", iInd)
            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("<")
            oNew.sDestination = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd).Trim)

            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("</td")

            If App.IsMobile And Not App.GetSettingsBool("stacjePosrednieRozklad") Then
                oNew.sPrzez = ""
            Else
                '<br> do </td> -> .sPrzez , "  " -> " "
                oNew.sPrzez = App.RemoveHtmlTags(sPage.Substring(0, iInd))
                oNew.sPrzez = oNew.sPrzez.Replace(vbLf, " ")
                oNew.sPrzez = oNew.sPrzez.Replace("  ", " ")
                ' oraz &#243; na ó etc.
                oNew.sPrzez = System.Net.WebUtility.HtmlDecode(oNew.sPrzez)
            End If

            iInd = sPage.IndexOf("<td")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf(">")
            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("<")
            oNew.sPeron = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd)).Trim

            App.mlPociagiStacji.Add(oNew)
            ' <td , >, do <br> -> sPeron
            iInd = sPage.IndexOf("<tr class")
        End While

    End Sub
    Private Async Function CreateUriMobile(bPrzyjazdy As Boolean) As Task(Of String)
        Dim sData As String = uiData.Date.ToString("dd.MM.yy")

        ' jesli znamy, to dodac warto PKPID, ktory uczyni jednoznaczne
        Dim idPkp As Integer = Await App.moPunkty.GetPKPIdAsync(msNazwa, False)
        Dim sStation As String = Uri.EscapeUriString(msNazwa)
        If idPkp > 0 Then
            sStation = sStation & "%230051" & idPkp.ToString("00000")
        End If

        Dim sUri As String = "http://mobil.rozklad-pkp.pl/bin/stboard.exe/pn" &
                "?maxJourneys=10&REQStationS0F=excludeStationAttribute%3BM-&selectDate=today" &
                "&dateBegin=" & sData &
                "&dateEnd=" & uiData.Date.AddMonths(1).ToString("dd.MM.yy") &
                "&input=" & sStation &
                "&time=" & uiCzas.Time.ToString("hh\%\3\Amm")


        If bPrzyjazdy Then
            sUri &= "&boardType=arr"
        Else
            sUri &= "&boardType=dep"
        End If

        sUri &= "&GUIREQProduct_0=on&GUIREQProduct_1=on&GUIREQProduct_2=on&GUIREQProduct_3=on&start=Anzeigen"

        Return sUri
    End Function
    Private Sub ImportTablicaStacjiMobile(sPage As String)
        Dim sLastDate As String = uiData.Date.ToString("dd")

        Dim iInd As Integer
        iInd = sPage.IndexOf("bold center sepline")
        While iInd > 0
            Dim oNew = New EntryRozkladu

            sPage = sPage.Substring(iInd + 5)

            '<td class="bold center sepline top">16:14</td>
            iInd = sPage.IndexOf(">")
            sPage = sPage.Substring(iInd + 1)

            oNew.sTime = sPage.Substring(0, 5)

            ' jesli data jest nie tego dnia... niekoniecznie to następny dzień!
            ' wersja mobile: nie ma tego znacznika, trzeba byłoby to rozpoznawać przez 'zmalenie' godziny
            oNew.sDate = ""

            ' a href="XX" jako .sLinkKurs
            iInd = sPage.IndexOf("href")
            sPage = sPage.Substring(iInd + 6)
            iInd = sPage.IndexOf("""")
            oNew.sLinkKurs = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd)) ' htmldecode, bo jest za duzo &amp; :)

            ' alt="XX" jako .sKurs
            '(choć może lepsze by było nie stąd a z 
            ' <td 
            ' </a , wstecz do > , -> .sKurs
            iInd = sPage.IndexOf("alt")
            sPage = sPage.Substring(iInd + 5)
            iInd = sPage.IndexOf("""")
            oNew.sKurs = sPage.Substring(0, iInd)


            iInd = sPage.IndexOf("<td")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("</a")
            iInd = sPage.LastIndexOf(">", iInd)
            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("<")
            oNew.sDestination = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd).Trim)

            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf("</td")

            If App.IsMobile And Not App.GetSettingsBool("stacjePosrednieRozklad") Then
                oNew.sPrzez = ""
            Else
                '<br> do </td> -> .sPrzez , "  " -> " "
                oNew.sPrzez = App.RemoveHtmlTags(sPage.Substring(0, iInd))
                oNew.sPrzez = oNew.sPrzez.Replace(vbCrLf, " ")
                oNew.sPrzez = oNew.sPrzez.Replace(vbLf, " ")
                oNew.sPrzez = oNew.sPrzez.Replace("  ", " ")
                oNew.sPrzez = System.Net.WebUtility.HtmlDecode(oNew.sPrzez)
                ' oraz &#243; na ó etc.
            End If

            iInd = sPage.IndexOf("<td")
            sPage = sPage.Substring(iInd)
            iInd = sPage.IndexOf(">")
            sPage = sPage.Substring(iInd + 1)
            iInd = sPage.IndexOf("<")
            oNew.sPeron = System.Net.WebUtility.HtmlDecode(sPage.Substring(0, iInd)).Trim

            App.mlPociagiStacji.Add(oNew)
            ' <td , >, do <br> -> sPeron
            iInd = sPage.IndexOf("bold center sepline")
        End While

    End Sub

    Private Async Sub ShowRozklad(bPrzyjazdy As Boolean)

        Dim bDesktop = False    ' przełącznik skąd importować

        If App.mlPociagiStacji Is Nothing OrElse mbLastType <> bPrzyjazdy Then

            Dim sUri As String
            If bDesktop Then
                sUri = Await CreateUriDesktop(bPrzyjazdy)
            Else
                sUri = Await CreateUriMobile(bPrzyjazdy)
            End If

            Dim sPage As String = Await App.HttpPageAsync(sUri, "rozklad")
            If sPage = "" Then Exit Sub

            ' przetwórz i pokaż
            ' teoretycznie kazdy powinien byc klikalny kazdy pociag
            ' pociag: contextmenu: show, makeCurrent (do sledzenia) 

            App.mlPociagiStacji = New Collection(Of EntryRozkladu)

            If bDesktop Then
                ImportTablicaStacjiDesktop(sPage)
            Else
                ImportTablicaStacjiMobile(sPage)
            End If

        End If

        uiListItems.ItemsSource = App.mlPociagiStacji
    End Sub

    Private Sub uiShowOdjazdy_Click(sender As Object, e As RoutedEventArgs)
        uiKiedy.Visibility = Visibility.Collapsed
        ShowRozklad(False)
    End Sub

    Private Sub uiShowPrzyjazdy_Click(sender As Object, e As RoutedEventArgs)
        uiKiedy.Visibility = Visibility.Collapsed
        ShowRozklad(True)
    End Sub

    Private Async Sub uiShowBrowser_Click(sender As Object, e As RoutedEventArgs)
        ' wymuszenie pokazania w zewnetrznej przegladarce - np. mozna otworzyc kilka stron
        Dim sUri As String

        If App.IsMobile Then
            sUri = Await CreateUriMobile(False)
        Else
            sUri = Await CreateUriDesktop(False)
        End If

        App.OpenBrowser(sUri, False)
        ' http://rozklad-pkp.pl/pl/sq?maxJourneys=10&input=Krak%C3%B3w+G%C5%82%C3%B3wny%23005100028&REQStationS0F=excludeStationAttribute%3BM-&disableEquivs=yes&date=30.07.18&dateStart=30.07.18&REQ0JourneyDate=30.07.18&time=23%3A31&boardType=dep&GUIREQProduct_0=on&GUIREQProduct_1=on&GUIREQProduct_2=on&GUIREQProduct_3=on&maxJourneys=10&dateEnd=30.07.18&advancedProductMode=&start=#focus

    End Sub

    Private Sub uiItem_Tapped(sender As Object, e As TappedRoutedEventArgs)
        ' tap na konkretnym pociągu
        Dim sLink As String = TryCast(TryCast(sender, Grid).DataContext, EntryRozkladu).sLinkKurs
        If sLink.Length < 20 Then
            App.DialogBox("ERROR: brak danych (sLink)?")
            Exit Sub
        End If
        ' minimalny działający link wraz z opoznieniami: http://rozklad-pkp.pl/pl/ti?trainlink=989637/515889/394252/132754/51&date=13.08.18
        Me.Frame.Navigate(GetType(OneTrain), sLink)
    End Sub

    Private Sub DateTimeChanged()
        If mbInsideInit Then Exit Sub
        App.mlPociagiStacji = Nothing   ' czyli musisz sobie na nowo sciagnac
    End Sub
    Private Sub uiDate_Changed(sender As Object, e As DatePickerValueChangedEventArgs) Handles uiData.DateChanged
        DateTimeChanged()
    End Sub

    Private Sub uiTime_Changed(sender As Object, e As TimePickerValueChangedEventArgs) Handles uiCzas.TimeChanged
        DateTimeChanged()
    End Sub
End Class
