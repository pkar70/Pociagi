' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.Devices.Geolocation
Imports Windows.Storage
Imports Windows.Web.Http
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Stacje
    Inherits Page

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Await App.InitPunkty(False) ' jak jeszcze nie ma, to wczytaj
        App.moPunkty.MarkShow("")
        uiListItems.ItemsSource = App.moPunkty.GetSource
        bInTextChange = False
    End Sub

    Private bInTextChange As Boolean

    Private Async Sub uiStacja_TextChanged(sender As Object, e As TextChangedEventArgs) Handles uiStacja.TextChanged
        If bInTextChange Then Exit Sub

        If App.GetSettingsBool("nazwyOnline") Then
            uiListItems.ItemsSource = Await App.GetOnlineStationNameAsync(uiStacja.Text, False)
        Else
            Dim sMask As String
            sMask = uiStacja.Text
            If sMask.Length < 3 Then sMask = ""
            App.moPunkty.MarkShow(sMask)
            uiListItems.ItemsSource = App.moPunkty.GetSource
        End If

        'Dim oItems = TryCast(App.moPunkty.GetSource, System.Linq.OrderedEnumerable(Of Pociagi.GeoPunkt, String))
        'If oItems.Count = 1 Then
        '    If App.GetSettingsBool("autoWpisNazwy") Then
        '        bInTextChange = True
        '        uiStacja.Text = oItems.Item(0).nazwa
        '        bInTextChange = False
        '    End If
        'End If
        ' jesli tylko jeden
        ' oraz App.GetSettingsBool("autoWpisNazwy")
        ' wtedy wpisz pełną nazwe (ale uwaga: zeby nie w petli sam sobie
    End Sub

    Private Async Function GetGPSpoint() As Task(Of String)

        Dim oNPos As Nullable(Of BasicGeoposition)
        oNPos = Await GeoPunkty.GetGPSpointAsync(True)
        If Not oNPos.HasValue Then Return ""

        Return "&lat=" & oNPos.Value.Latitude & "&lon=" & oNPos.Value.Longitude

    End Function

    Private Async Function GetNearestOnlineUsingBazaAsync() As Task(Of String)
        Dim sPoint As String = Await GetGPSpoint()
        If sPoint = "" Then Return ""

        ' (1) zamiana punktu na link do bazy
        ' https://www.bazakolejowa.pl/index.php?xspec=1&dzial=lokalizuj&lat=50.0&lon=19.9
        ' -->
        ' index.php?dzial= Stacjeid=15324

        Dim sUri As String = "https://www.bazakolejowa.pl/index.php?xspec=1&dzial=lokalizuj" & sPoint

        Dim sResp As String = Await App.HttpPageAsync(sUri, "getnearest")
        If sResp = "" Then Return ""

        ' index.php?dzial=Stacje&id=15324

        ' (2) zamiana linku do bazy na nazwę stacji
        Dim iInd As Integer = sResp.LastIndexOf("=")
        sResp = App.moPunkty.GetName(False, sResp.Substring(iInd + 1))

        If sResp = "" Then
            ' nie mamy w lokalnej bazie tego id, to sprawdzam w sieci
            ' https://www.bazakolejowa.pl/index.php?dzial=stacje&id=15324
            ' -->
            ' Kraków Opatkowice
            sUri = "https://www.bazakolejowa.pl/" & sResp.Trim
            sResp = Await App.HttpPageAsync(sUri, "getname")
            If sResp = "" Then Return ""
            iInd = sResp.IndexOf("<title>")
            If iInd < 10 Then Return ""
            sResp = sResp.Substring(iInd + 7)
            iInd = sResp.IndexOf("<")
            If iInd < 1 Then Return ""
            sResp = sResp.Substring(0, iInd)
        End If

        Return sResp

    End Function

    Private Async Sub uiGetGPS_Click(sender As Object, e As RoutedEventArgs)

        Dim sName As String
        ' sName = GetNearestOnlineUsingBazaAsync()
        ' If sName = "" then exit sub

        Dim oItem As GeoPunkt
        ' App.moPunkty.ShowInRadix(App.GetSettingsInt("nearestRadix"), oNPos.Value)

        oItem = Await App.moPunkty.GetNearestAsync(App.GetSettingsInt("nearestRadix", 5))
        If oItem Is Nothing Then Exit Sub
        sName = oItem.nazwa

        bInTextChange = True    ' zmiana Text nie spowoduje zmiany listy
        uiStacja.Text = sName
        ' bInTextChange = False ' tyle ze to wszystko Async, więc OnTextChange jest po tym :)

        uiListItems.ItemsSource = App.moPunkty.GetSource
    End Sub

    Private Sub PokazRozkladStacji(sName As String)
        App.mlPociagiStacji = Nothing   ' zeby na pewno wczytał, nie korzystał z cache
        Me.Frame.Navigate(GetType(RozkladStacji), sName)
    End Sub

    Private Sub uiItem_Tapped(sender As Object, e As TappedRoutedEventArgs)
        PokazRozkladStacji(TryCast(sender, TextBlock).Text)
    End Sub

    Private Sub uiPokazRozklad_Click(sender As Object, e As RoutedEventArgs)
        PokazRozkladStacji(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, GeoPunkt).nazwa)
    End Sub

    Private Sub uiGoAtlas_Click(sender As Object, e As RoutedEventArgs)
        '       "Przejdź do atlaskolejowy" (do wyszukiwarki?)   
        '           http://pl.atlaskolejowy.net/mazowieckie/?id=baza&poz=3980
        '           busko: http://pl.atlaskolejowy.net/swietokrzyskie/?id=baza&poz=2522
        '               (ale mozna wykasowac 'swietokrzyskie' i tez zadziała)

        Dim iId As Integer
        Dim oItem As GeoPunkt = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, GeoPunkt)

        If App.GetSettingsBool("nazwyOnline") Then
            Dim sName As String = oItem.nazwa
            iId = App.moPunkty.GetAtlasId(sName)
        Else
            iId = oItem.idAtlas
        End If

        If iId = 0 Then
            ' nie znamy identyfikatora, strona szukania
            Dim sName As String = oItem.nazwa
            ' *TODO* to nie dziala, tam jest POST
            App.OpenBrowser("http://pl.atlaskolejowy.net/wynik.php?x=1&y=1&szukaj=" & sName, False)
        Else
            App.OpenBrowser("http://pl.atlaskolejowy.net/?id=baza&poz=" & iId, False)
        End If

    End Sub

    Private Sub uiGoBaza_Click(sender As Object, e As RoutedEventArgs)
        '       "Przejdź do bazakolejowa" (do wyszukiwarki? bo skąd miałbym id?)
        '           https://www.bazakolejowa.pl/index.php?dzial=stacje&id=7358&okno=start
        '           busko: https://www.bazakolejowa.pl/index.php?dzial=stacje&id=636&okno=start

        Dim iId As Integer
        Dim oItem As GeoPunkt = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, GeoPunkt)

        If App.GetSettingsBool("nazwyOnline") Then
            Dim sName As String = oItem.nazwa
            iId = App.moPunkty.GetBazaId(sName)
        Else
            iId = oItem.idBaza
        End If

        If iId = 0 Then
            ' nie znamy identyfikatora, strona szukania
            Dim sName As String = oItem.nazwa
            App.OpenBrowser("https://www.bazakolejowa.pl/index.php?dzial=szukaj&rodzaj=s&szukaj=" & Uri.EscapeUriString(sName), False)
        Else
            App.OpenBrowser("https://www.bazakolejowa.pl/index.php?dzial=stacje&id=" & iId, False)
        End If

    End Sub

    Private Async Sub uiFavourites_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, GeoPunkt)

        Dim oFile As StorageFile
        oFile = Await App.GetDataFile("favPoints.txt", True)
        If oFile Is Nothing Then Exit Sub   ' nie da sie, bo nie ma pliku i nie mozna utworzyc

        If oItem.sWyroznik.IndexOf("*") > -1 Then
            ' usun z favourites
            Dim sFav As String = Await FileIO.ReadTextAsync(oFile)
            sFav = sFav.Replace(oItem.nazwa & vbCrLf, "")
            Await FileIO.WriteTextAsync(oFile, sFav)
            oItem.sWyroznik = ""
        Else
            ' dodaj do favourites
            Await FileIO.AppendTextAsync(oFile, oItem.nazwa & vbCrLf)
            oItem.sWyroznik = "*"
        End If

        uiStacja_TextChanged(Nothing, Nothing)
    End Sub

    ' "...":
    ' * pokaż także nie osobowe (w wyszukiwarce)
    ' * pokaż najbliższe z GPS, zgodnie z GetSettInt("radix")?

    ' może być sprawdzanie danych stacji, np. na beskid plik Xml, kopiowany do lokalnego

    ' ze stacji, data, czas, odjazdy/przyjazdy

End Class
