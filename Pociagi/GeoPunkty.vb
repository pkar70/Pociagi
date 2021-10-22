Imports System.Xml.Serialization
Imports Windows.Devices.Geolocation
Imports Windows.Storage

<XmlType("stacja")>
Public Class GeoPunkt
    ' połączenie bazy punktów, faworytów, oraz GPSów (do pokazywania w liście)
    <XmlAttribute()>
    Public Property nazwa As String
    <XmlAttribute()>
    Public Property type As String
    <XmlAttribute()>
    Public Property idBaza As Integer
    <XmlAttribute()>
    Public Property idAtlas As Integer
    <XmlAttribute()>
    Public Property idPKP As Integer
    <XmlAttribute()>
    Public Property lat As Double = 100 ' oznacza BRAK
    <XmlAttribute()>
    Public Property lon As Double = 0

    <XmlIgnore>
    Public Property sWyroznik As String = ""    ' puste to punkt z listy PKP, gwiazdka to z favourites, a pin to z GPS
    <XmlIgnore>
    Public Property bShow As Boolean = False
End Class


Public Class GeoPunkty
    Private moPunkty As Collection(Of GeoPunkt) = Nothing
    Private mbDirty As Boolean

    Public Sub Clear()
        moPunkty = New Collection(Of GeoPunkt)
        mbDirty = False
    End Sub

    Sub New()
        Clear()
    End Sub

    Private Function LoadPunkty() As Boolean
        If Not File.Exists("Assets\listaPunktow.xml") Then Return False

        Dim oStream As StreamReader = File.OpenText("Assets\listaPunktow.xml")
        Dim oSer = New XmlSerializer(GetType(Collection(Of GeoPunkt)))

        moPunkty = TryCast(oSer.Deserialize(oStream), Collection(Of GeoPunkt))

        For Each oItem As GeoPunkt In moPunkty
            oItem.bShow = False
            oItem.sWyroznik = ""
        Next

        Return True
    End Function

    Private Async Function MarkFaworAsync() As Task(Of Integer)
        ' zaznaczenie sWyroznik = "*"
        ' zwraca liczbę Favourites (znalezionych!)

        Dim oFile As StorageFile
        oFile = Await App.GetDataFile("favPoints.txt", False)
        If oFile Is Nothing Then Return False

        Dim sFav As String = Await FileIO.ReadTextAsync(oFile)
        Dim iCnt As Integer = 0

        Dim aArr As String() = sFav.Split(vbCrLf)
        For Each sTmp As String In aArr
            sTmp = sTmp.Trim
            For Each oItem As GeoPunkt In moPunkty
                If oItem.nazwa = sTmp Then
                    oItem.sWyroznik = "*"
                    iCnt += 1
                    Exit For
                End If
            Next
        Next

        Return iCnt
    End Function

    Public Sub MarkShow(sMask As String)
        ' zaznacz te, które są faworytami / te które pasują do wzorca (gdy jest podany)

        If sMask Is Nothing Then sMask = ""
        sMask = sMask.ToLower

        For Each oItem As GeoPunkt In moPunkty
            oItem.bShow = False
            If sMask <> "" Then
                If oItem.nazwa.ToLower.IndexOf(sMask) > -1 Then oItem.bShow = True
            Else
                If oItem.sWyroznik.IndexOf("*") > -1 Then oItem.bShow = True
            End If
        Next

    End Sub

    Public Async Function LoadAsync() As Task(Of Boolean)
        Clear() ' najpierw wyczyść
        If Not LoadPunkty() Then Return False

        Await MarkFaworAsync()

        Return True
    End Function

    Public Async Function SaveAsync(Optional bForce As Boolean = False) As Task(Of Boolean)
        Throw New NotImplementedException
    End Function

    Public Function GetSource()
        If moPunkty.Count = 0 Then Return Nothing
        Return From c In moPunkty Order By c.nazwa Where c.bShow = True
        'Return From c In moPunkty Where c.bShow = True
    End Function
    Public Function GetSourceNames()
        If moPunkty.Count = 0 Then Return Nothing
        Return From c In moPunkty Order By c.nazwa Where c.bShow = True Select c.nazwa
    End Function
    Public Function GetRawSource()
        If moPunkty.Count = 0 Then Return Nothing
        Return From c In moPunkty
    End Function

    Public Function Count() As Integer
        Return moPunkty.Count
    End Function

    Public Function GetId(sName As String, bAtlas As Boolean) As Integer
        For Each oItem As GeoPunkt In moPunkty
            If sName = oItem.nazwa Then
                If bAtlas Then Return oItem.idAtlas
                Return oItem.idBaza
            End If
        Next
        Return 0
    End Function
    Public Async Function GetPKPIdAsync(sName As String, bGetOnline As Boolean) As Task(Of Integer)
        For Each oItem As GeoPunkt In moPunkty
            If sName = oItem.nazwa Then
                If oItem.idPKP <> 0 Then Return oItem.idPKP
                If Not bGetOnline Then Return 0
                Return Await GetOnlinePkpIdAsync(sName)
            End If
        Next
        Return 0
    End Function

    Public Sub SetId(sName As String, bAtlas As Boolean, iId As Integer)
        For Each oItem As GeoPunkt In moPunkty
            If sName = oItem.nazwa Then
                If bAtlas Then
                    If oItem.idAtlas = 0 Then oItem.idAtlas = iId
                Else
                    If oItem.idBaza = 0 Then oItem.idBaza = iId
                End If
                Exit For
            End If
        Next

    End Sub
    Public Function GetName(bAtlas As Boolean, iId As Integer) As String
        For Each oItem As GeoPunkt In moPunkty
            If bAtlas Then
                If oItem.idAtlas = iId Then Return oItem.nazwa
            Else
                If oItem.idBaza = iId Then Return oItem.nazwa
            End If
        Next
        Return ""
    End Function

    Private Async Function GetOnlinePkpIdAsync(sName As String) As Task(Of Integer)
        If Not App.IsNetIPavailable(False) Then Return 0
        Dim sUrl As String = "http://rozklad-pkp.pl/station/search?short=0&term=" & Uri.EscapeUriString(sName)
        Dim sJSON As String = Await App.HttpPageAsync(sUrl, "getting PKP id")

        Dim iInd As Integer
        iInd = sJSON.IndexOf("value"":""51")
        If iInd < 1 Then Return 0
        sJSON = sJSON.Substring(iInd + 10)
        Return sJSON.Substring(0, 5)
    End Function

    Private Function GPSdistanceDwa(dLat0 As Double, dLon0 As Double, dLat As Double, dLon As Double) As Integer
        ' https://stackoverflow.com/questions/28569246/how-to-get-distance-between-two-locations-in-windows-phone-8-1

        Dim iRadix As Integer = 6371000
        Dim tLat As Double = (dLat - dLat0) * Math.PI / 180
        Dim tLon As Double = (dLon - dLon0) * Math.PI / 180
        Dim a As Double = Math.Sin(tLat / 2) * Math.Sin(tLat / 2) +
            Math.Cos(Math.PI / 180 * dLat0) * Math.Cos(Math.PI / 180 * dLat) *
            Math.Sin(tLon / 2) * Math.Sin(tLon / 2)
        Dim c As Double = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)))
        Dim d As Double = iRadix * c

        Return d
    End Function

    Public Function GetNearest(dLat As Double, dLon As Double, iRadix As Integer) As GeoPunkt
        Dim oTmp As GeoPunkt = Nothing
        Dim iMinOdl As Integer = 800 * 1000 ' 600 km
        iRadix *= 1000

        ' lat=50.06143N, lon=19.93658E
        For Each oItem As GeoPunkt In moPunkty
            Dim iOdl As Integer
            iOdl = GPSdistanceDwa(oItem.lat, oItem.lon, dLat, dLon)
            If iOdl < iMinOdl Then
                iMinOdl = iOdl
                oTmp = oItem
            End If
            If iRadix > 0 Then
                If iOdl < iRadix Then oItem.bShow = True
            End If
        Next

        oTmp.bShow = True

        Return oTmp
    End Function

    Public Async Function GetNearestAsync(iRadix As Integer) As Task(Of GeoPunkt)

        Dim oNPos As Nullable(Of BasicGeoposition)
        oNPos = Await GeoPunkty.GetGPSpointAsync(True)
        If Not oNPos.HasValue Then Return Nothing

        'Return ShowInRadix(iRadix, oNPos.Value)

        Return GetNearest(oNPos.Value.Latitude, oNPos.Value.Longitude, iRadix)

    End Function

    Public Shared Async Function GetGPSpointAsync(Optional bLocalError As Boolean = True) As Task(Of BasicGeoposition?)

        Dim rVal As Windows.Devices.Geolocation.GeolocationAccessStatus = Await Geolocator.RequestAccessAsync()
        If rVal <> GeolocationAccessStatus.Allowed Then
            If bLocalError Then App.DialogBox("resErrorNoGPSAllowed")
            Return Nothing
        End If

        Dim oDevGPS As Geolocator = New Geolocator()

        Dim oPos As Geoposition = Nothing

        oDevGPS.DesiredAccuracyInMeters = App.GetSettingsInt("gpsPrec", 200)
        Dim oCacheTime As TimeSpan = New TimeSpan(0, 1, 0)  ' minuta 
        Dim oTimeout As TimeSpan = New TimeSpan(0, 0, 3)    ' timeout 
        Dim bErr As Boolean = False
        Dim sUri As String = ""
        Try
            oPos = Await oDevGPS.GetGeopositionAsync(oCacheTime, oTimeout)
        Catch ex As Exception   ' zapewne timeout
            bErr = True
        End Try

        If bErr Then
            If bLocalError Then App.DialogBox("resErrorGettingPos")
            Return Nothing
        End If

        Return oPos.Coordinate.Point.Position
    End Function

    'Public Function ShowInRadix(iRadix As Integer, oPos As BasicGeoposition) As Task(Of BasicGeoposition)

    '    Dim iMinOdl As Integer = App.GetSettingsInt("nearestRadix") * 1000

    '    For Each oItem As GeoPunkt In moPunkty
    '        If GPSdistanceDwa(oItem.lat, oItem.lon, oPos.Latitude, oPos.Longitude) < iMinOdl Then
    '            oItem.bShow = True
    '        End If
    '    Next

    'End Function

    Public Function GetAtlasId(sName As String) As Integer
        ' uzywane, gdy mamy nazwy z online, i chcemy odnalezc to w bazie
        For Each oItem As GeoPunkt In moPunkty
            If oItem.nazwa = sName Then Return oItem.idAtlas
        Next
        Return 0
    End Function

    Public Function GetBazaId(sName As String) As Integer
        For Each oItem As GeoPunkt In moPunkty
            If oItem.nazwa = sName Then Return oItem.idBaza
        Next
        Return 0
    End Function

    Public Async Function DownloadPkpId() As Task
        For Each oItem As GeoPunkt In moPunkty
            If oItem.idPKP = 0 Then
                oItem.idPKP = Await GetPKPIdAsync(oItem.nazwa, True)
            End If
        Next

        'Dim oStream As StreamReader = File.OpenText("Assets\listaPunktow.xml")
        'Dim oSer = New XmlSerializer(GetType(Collection(Of GeoPunkt)))

    End Function
End Class
