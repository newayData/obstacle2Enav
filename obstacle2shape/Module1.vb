﻿Imports System.IO
Imports System.Text

Imports DotSpatial.Data

Imports DotSpatial.Topology


Module Module1


    Dim groupThrNm As Double = 1

    ' header
    Sub writeHeader()
        Console.BackgroundColor = ConsoleColor.Gray
        Console.ForegroundColor = ConsoleColor.Black

        Console.WriteLine("(c) neway data AG - Landstraße 105 - 9490 Vaduz - FL-0002.103.140-4")
        Console.WriteLine("OBSTACLE2SHAPE CONVERTER" & "    VERSION: " & Version & " ")
        Console.ResetColor()
    End Sub

    Public decimalSeparator As String = Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator


    Dim Version = "1.5"
    Dim csvFile As String = ""
    Sub Main(args As String())
        writeHeader()

        ' get args
        Dim lastItem As String = ""
        For Each item In args
            If lastItem.Contains("--f") Then
                csvFile = item
            End If

            lastItem = item
        Next

        If System.IO.File.Exists(csvFile) Then
            Console.WriteLine("use file: " & csvFile)
        Else
            Console.WriteLine("ERR file: " & csvFile & " not found")
        End If

        createElements(parseCsv())

        ' create shape
        createShape()

    End Sub

    Sub createShape()
        Dim fs As New FeatureSet(FeatureType.Line)
        fs.DataTable.Columns.Add(New DataColumn("id", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("type", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("name", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("description", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("label", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("height", Type.GetType("System.Int32")))
        fs.DataTable.Columns.Add(New DataColumn("elevation", Type.GetType("System.Int32")))
        fs.DataTable.Columns.Add(New DataColumn("markingText", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("origin", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("lighted", Type.GetType("System.String")))
        fs.DataTable.Columns.Add(New DataColumn("marked", Type.GetType("System.String")))


        ' group point
        Dim fsx As New FeatureSet(FeatureType.Point)
        fsx.DataTable.Columns.Add(New DataColumn("id", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("type", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("name", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("description", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("label", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("height", Type.GetType("System.Int32")))
        fsx.DataTable.Columns.Add(New DataColumn("elevation", Type.GetType("System.Int32")))
        fsx.DataTable.Columns.Add(New DataColumn("markingText", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("origin", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("lighted", Type.GetType("System.String")))
        fsx.DataTable.Columns.Add(New DataColumn("marked", Type.GetType("System.String")))
        ' groups
        For Each cli In group

            Dim listCl As New List(Of Coordinate)

            Dim descr = ""
            Dim maxHeight As Double = 0
            Dim height As String = ""

            Dim dpfs As New List(Of DoublePointStruct)
            Dim lighted As Boolean = False

            For Each q In cli
                Dim dp = New DoublePointStruct
                dp.x = q.lon
                dp.y = q.lat


                ' check point is there
                Dim pointisthere As Boolean = False
                For Each ppa In dpfs
                    If ppa.x = dp.x And ppa.y = dp.y Then
                        pointisthere = True
                        Exit For
                    End If
                Next

                If Not pointisthere Then dpfs.Add(dp)

                descr = q.description

                If q.height > maxHeight Then
                    maxHeight = q.height
                    height = "max " & q.height & q.heightUnit
                End If

                If q.lighted Then lighted = True

            Next

            Dim cen = FindCentroid(dpfs.ToArray)

            Dim ffg As IFeature = fsx.AddFeature(New Point(cen.x, cen.y))

            ffg.DataRow("name") = descr
            ffg.DataRow("label") = height & " " & descr
            ffg.DataRow("height") = maxHeight
            ffg.DataRow("lighted") = lighted
            ffg.DataRow("marked") = False

            ffg.DataRow.AcceptChanges()

            Dim res = makePolygonConcarve(dpfs.ToArray)
            For Each p In res
                If p.x = 0 And p.y = 0 Then
                Else
                    Dim cl As New Coordinate(p.x, p.y)
                    listCl.Add(cl)
                End If

            Next



            If listCl.Count > 1 Then
                Dim ffa As IFeature = fs.AddFeature(New Polygon(listCl))

                ffa.DataRow("name") = descr
                ffa.DataRow("label") = height & " " & descr
                ffa.DataRow("height") = maxHeight
                ffa.DataRow("lighted") = lighted
                ffa.DataRow("marked") = False
                ffa.DataRow.AcceptChanges()
            End If

        Next

        fsx.SaveAs("out\groupedPoint.shp", True)
        fs.SaveAs("out\groupedPolyLine.shp", True)

        ' single obstacles
        Dim fsS As New FeatureSet(FeatureType.Point)
        fsS.DataTable.Columns.Add(New DataColumn("id", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("type", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("name", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("description", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("label", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("height", Type.GetType("System.Int32")))
        fsS.DataTable.Columns.Add(New DataColumn("elevation", Type.GetType("System.Int32")))
        fsS.DataTable.Columns.Add(New DataColumn("markingText", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("origin", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("lighted", Type.GetType("System.String")))
        fsS.DataTable.Columns.Add(New DataColumn("marked", Type.GetType("System.String")))

        For Each cli In singleGroup
            Dim cl As New Coordinate(cli.lon, cli.lat)
            Dim ffa As IFeature = fsS.AddFeature(New Point(cl))

            ffa.DataRow("name") = cli.name
            ffa.DataRow("type") = cli.type
            ffa.DataRow("description") = cli.description
            ffa.DataRow("markingText") = cli.markingText
            ffa.DataRow("origin") = cli.origin
            ffa.DataRow("lighted") = cli.lighted
            ffa.DataRow("label") = "max " & cli.height & cli.heightUnit & " " & cli.description
            ffa.DataRow("height") = cli.height
            ffa.DataRow("marked") = cli.marked
            ffa.DataRow.AcceptChanges()
        Next

        fsS.SaveAs("out\groupedSingle.shp", True)



        ' all point obstalces
        ' +++++++++++++++++++++

        ' single obstacles
        Dim singleObs As New FeatureSet(FeatureType.Point)
        singleObs.DataTable.Columns.Add(New DataColumn("id", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("type", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("name", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("description", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("label", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("height", Type.GetType("System.Int32")))
        singleObs.DataTable.Columns.Add(New DataColumn("elevation", Type.GetType("System.Int32")))
        singleObs.DataTable.Columns.Add(New DataColumn("markingText", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("origin", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("lighted", Type.GetType("System.String")))
        singleObs.DataTable.Columns.Add(New DataColumn("marked", Type.GetType("System.String")))

        For Each cli In singleGroup
            Dim cl As New Coordinate(cli.lon, cli.lat)
            Dim ffa As IFeature = singleObs.AddFeature(New Point(cl))
            ffa.DataRow("id") = cli.id
            ffa.DataRow("name") = cli.name
            ffa.DataRow("type") = cli.type
            ffa.DataRow("description") = cli.description
            ffa.DataRow("markingText") = cli.markingText
            ffa.DataRow("label") = cli.height & cli.heightUnit & " " & cli.description
            ffa.DataRow("height") = cli.height
            ffa.DataRow("origin") = cli.origin
            ffa.DataRow("lighted") = cli.lighted
            ffa.DataRow("marked") = cli.marked
            ffa.DataRow.AcceptChanges()
        Next

        ' groups
        For Each q In group


            For Each cli In q
                Dim cl As New Coordinate(cli.lon, cli.lat)
                Dim ffa As IFeature = singleObs.AddFeature(New Point(cl))


                ffa.DataRow("name") = cli.name
                ffa.DataRow("type") = cli.type
                ffa.DataRow("description") = cli.description
                ffa.DataRow("markingText") = cli.markingText
                ffa.DataRow("origin") = cli.origin
                ffa.DataRow("label") = cli.height & cli.heightUnit & " " & cli.description
                ffa.DataRow("height") = cli.height
                ffa.DataRow("lighted") = cli.lighted
                ffa.DataRow("marked") = cli.marked
                ffa.DataRow.AcceptChanges()

            Next
        Next

        singleObs.SaveAs("out\allPoints.shp", True)




        ' line obstacles
        Dim fsL As New FeatureSet(FeatureType.Line)
        fsL.DataTable.Columns.Add(New DataColumn("id", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("type", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("name", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("description", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("label", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("height", Type.GetType("System.Int32")))
        fsL.DataTable.Columns.Add(New DataColumn("elevation", Type.GetType("System.Int32")))
        fsL.DataTable.Columns.Add(New DataColumn("markingText", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("origin", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("lighted", Type.GetType("System.String")))
        fsL.DataTable.Columns.Add(New DataColumn("marked", Type.GetType("System.String")))



        Dim fs_pylon As New FeatureSet(FeatureType.Point)


            For Each clf In lines

                Dim maxHei As Double = 0
                Dim listCl As New List(Of Coordinate)
                Dim lighte As Boolean


                For Each cli In clf





                    ' evaluate limits
                    Dim cl As New Coordinate(cli.lon, cli.lat)

                    If cli.height > maxHei Then maxHei = cli.height
                    If cli.lighted Then lighte = True
                    If cli.type.ToLower = "MAST".ToLower Then


                        ' make pylon symbol
                        Dim angle As Double = 0
                        Dim dist As Double = 0.02
                        Dim p(4) As Coordinate

                        For i As Short = 0 To 3
                            Dim cp As New DoublePointStruct
                            cp.x = cli.lon
                            cp.y = cli.lat
                            Dim cd = GetRadialCoordinates(cp, angle, dist)

                            angle += 90
                            p(i) = New Coordinate(cd.x, cd.y)

                            'Console.Write("|")
                        Next
                        p(4) = p(0)
                        Dim lsf As New LineString(p)
                        Dim frf = New Feature(lsf)
                        Dim ffga As IFeature = fs_pylon.AddFeature(frf)
                    End If

                    listCl.Add(cl)
                Next

            If doPylons Then fs_pylon.SaveAs("out\linePylon.shp", True)


            If listCl.Count > 1 Then
                    Dim ffa As IFeature = fsL.AddFeature(New LineString(listCl))
                ffa.DataRow("id") = clf(0).id
                ffa.DataRow("name") = clf(0).name
                ffa.DataRow("type") = clf(0).type
                ffa.DataRow("description") = clf(0).description
                ffa.DataRow("markingText") = clf(0).markingText
                ffa.DataRow("lighted") = clf(0).lighted
                ffa.DataRow("label") = "max " & maxHei & clf(0).heightUnit & " " & clf(0).description
                ffa.DataRow("height") = maxHei
                ffa.DataRow("origin") = clf(0).origin
                ffa.DataRow("marked") = clf(0).marked
                ffa.DataRow.AcceptChanges()
                End If


            Next




            fsL.SaveAs("out\allLine.shp", True)


        ' write feature code file

        Dim file As System.IO.StreamWriter
        file = My.Computer.FileSystem.OpenTextFileWriter("out/featureCodes.txt", False)

        file.WriteLine("[Appearance]")
        file.WriteLine("FeatureClass=allPoints*,type=CHIMNEY,310")
        file.WriteLine("FeatureClass=allPoints*,type=TOWER,311")
        file.WriteLine("FeatureClass=allPoints*,type=WINDTURBINE,312")
        file.WriteLine("FeatureClass=allPoints*,type=MAST,313")
        file.WriteLine("FeatureClass=allPoints*,type=CRANE,314")
        file.WriteLine("FeatureClass=allPoints*,type=ANTENNA,315")


        ' group
        file.WriteLine("FeatureClass=groupedPoint*,lighted=true,330") 'general group obstacle -> lighted
        file.WriteLine("FeatureClass=groupedPoint*,lighted=false,331") 'general group obstacle -> NOT lighted
        file.WriteLine("FeatureClass=groupedSingle*,lighted=true,332") 'single group obstacle -> lighted
        file.WriteLine("FeatureClass=groupedSingle*,lighted=false,333") 'single group obstacle -> NOT lighted
        file.WriteLine("FeatureClass=groupedPolyLine*,336")

        ' pylon
        file.WriteLine("FeatureClass=linePylon*,321")

        ' line trusted source
        file.WriteLine("FeatureClass=allLine*,origin=bazl,334") 'powerline trusted
        file.WriteLine("FeatureClass=allLine*,origin=openstreetmap,335") ' Building (Aerial way so whatsoever)
        file.WriteLine("FeatureClass=allLine*,origin=cis_oeamtc,335") ' Building (Aerial way so whatsoever)
        file.WriteLine("FeatureClass=allLine*,origin=Kabeldaten Uri,335") ' Building (Aerial way so whatsoever)
        file.WriteLine("FeatureClass=allLine*,origin=Forstamt Tessin,335") ' Building (Aerial way so whatsoever)

        file.WriteLine("[Label]")
        file.WriteLine("FeatureClass=allPoints*,height")
        file.WriteLine("FeatureClass=groupedSingle*,height")
        file.WriteLine("FeatureClass=groupedPoint*,height")

        file.Close()



    End Sub

    Structure shapElementStruct
        Dim id As String
        Dim type As String
        Dim description As String
        Dim name As String
        Dim markingText As String
        Dim marked As Boolean
        Dim height As Long
        Dim lighted As Boolean
        Dim heightUnit As String
        Dim elevation As Long
        Dim validUntil As String
        Dim effectiveFrom As String
        Dim origin As String
        Dim lat As Double
        Dim lon As Double
    End Structure

    Dim group As New List(Of List(Of shapElementStruct))
    Dim singleGroup As New List(Of shapElementStruct)
    Dim lines As New List(Of List(Of shapElementStruct))


    Dim doGrouping As Boolean = False
    Dim doPylons As Boolean = False

    Sub createElements(data() As csvStruct)




        ' make all group obstacles)
        ' +++++++++++++++++++++++++


        Dim itemCnt As Long = 0
        For Each item In data
            itemCnt += 1

            Dim perc = Math.Round(itemCnt / data.Length, 4) * 100
            'Console.WriteLine(perc & " % finished")


            If item._used = False Then
                ' take all those, that have no linktype, or linktype group
                Select Case item.linkType.ToUpper
                    Case "GROUP", ""
                        Dim type = item.type


                        ' find all "types" within n nm
                        Dim center As New DoublePointStruct
                        Dim coordLst As New List(Of DoublePointStruct)
                        Dim groupObstacles As New List(Of shapElementStruct)

                        Dim noGroupFound As Boolean = True
up:

                        Dim cmm As Long = 0

                        If doGrouping Then


                            For Each item2 In data
                                If item2._used = False Then
                                    If (item2.linkType = "" Or item2.linkType = "GROUP") Then
                                        If item2.type.ToUpper = item.type.ToUpper Then

                                            ' calc dist
                                            center.x = item.longitude
                                            center.y = item.latitude

                                            If coordLst.Count = 0 Then
                                                coordLst.Add(center)
                                            End If

                                            Dim pos2 As New DoublePointStruct
                                            pos2.x = item2.longitude
                                            pos2.y = item2.latitude

                                            Dim inRange As Boolean = False


                                            If doGrouping Then
                                                For Each ff In coordLst
                                                    Dim dist = GetGreatCircleDistance_ConstEarthRadiusInNm(ff.x, pos2.x, ff.y, pos2.y)
                                                    If dist < groupThrNm And dist <> 0 Then
                                                        inRange = True
                                                        Exit For
                                                    End If
                                                Next
                                            Else
                                                noGroupFound = True

                                            End If



                                            If inRange Then

                                                noGroupFound = False
                                                'Console.WriteLine("added to group: " & type & "  |   distance to center of group: " & dist & " [Nm]")

                                                ' the first point
                                                If coordLst.Count = 1 Then
                                                    ' Console.WriteLine("group element: " & type)

                                                    Dim el As New shapElementStruct
                                                    el.name = item.name
                                                    el.id = item.codeGroup

                                                    el.origin = item.origin.ToLower
                                                    el.type = item.type.ToLower
                                                    el.description = item.groupDescription
                                                    el.markingText = item.markingDescription.ToLower
                                                    el.validUntil = item.validUntil
                                                    el.lat = item.latitude
                                                    el.lon = item.longitude
                                                    el.height = item.heightValue
                                                    el.heightUnit = item.heightUnit

                                                    el.marked = item.marked
                                                    el.lighted = item.lighted
                                                    groupObstacles.Add(el)

                                                End If

                                                Console.Write("*")

                                                ' all others
                                                Dim el2 As New shapElementStruct
                                                el2.name = item2.name
                                                el2.id = item.codeGroup

                                                el2.origin = item2.origin
                                                el2.type = item2.type
                                                el2.markingText = item2.markingDescription
                                                el2.validUntil = item2.validUntil
                                                el2.lat = item2.latitude
                                                el2.lon = item2.longitude
                                                el2.height = item2.heightValue
                                                el2.heightUnit = item2.heightUnit
                                                el2.marked = item2.marked
                                                el2.description = item2.groupDescription
                                                el2.lighted = item2.lighted
                                                groupObstacles.Add(el2)



                                                coordLst.Add(pos2)

                                                ' calc new center
                                                Dim cn As Short = 0
                                                Dim newC As New DoublePointStruct
                                                For Each c In coordLst
                                                    newC.x += c.x
                                                    newC.y += c.y
                                                    cn += 1
                                                Next

                                                center.x = newC.x / cn
                                                center.y = newC.y / cn
                                                data(cmm)._used = True

                                                GoTo up

                                            End If
                                        End If
                                    End If
                                End If

                                cmm += 1
                            Next
                        End If

                        If noGroupFound = False Then
                            If groupObstacles.Count > 0 Then
                                If group.Contains(groupObstacles) = False Then group.Add(groupObstacles)
                            End If
                        Else

                            ' this is a single obstacle without group
                            Dim el2 As New shapElementStruct
                            el2.name = item.name
                            el2.id = item.codeGroup

                            el2.origin = item.origin
                            el2.type = item.type
                            el2.markingText = item.markingDescription
                            el2.validUntil = item.validUntil
                            el2.lat = item.latitude
                            el2.description = item.groupDescription
                            el2.lon = item.longitude
                            el2.marked = item.marked
                            el2.height = item.heightValue
                            el2.heightUnit = item.heightUnit
                            el2.lighted = item.lighted
                            groupObstacles.Add(el2)

                            singleGroup.Add(el2)
                            Console.Write("#")
                        End If

                End Select

            End If
        Next




        ' lines
        ' +++++++++++++++++++++++++
        Dim usedsItems As Long = 0
        Dim cm As Long = 0
        Dim percOld As Short = 0
        For Each item In data


            Dim perc = Math.Round(usedsItems / data.Count, 4) * 100
            If CType(perc, Integer) <> percOld Then
                percOld = perc
                Console.WriteLine(perc & "% finished")
            End If

            If item._used = False Then
                ' take all those, that have no linktype, or linktype group
                Select Case item.linkType.ToUpper

                    Case "CABLE", "SOLID"


                        Dim type = item.type

                        Dim lineF As New List(Of shapElementStruct)

                        ' the first point

                        Dim el As New shapElementStruct
                        el.name = item.name
                        el.id = item.codeGroup

                        el.origin = item.origin
                        el.type = item.type
                        el.markingText = item.markingDescription
                        el.validUntil = item.validUntil
                        el.lat = item.latitude
                        el.lon = item.longitude
                        el.height = item.heightValue
                        el.heightUnit = item.heightUnit
                        el.description = item.groupDescription
                        el.lighted = item.lighted
                        el.marked = item.marked
                        lineF.Add(el)

                        'Console.Write("  T")

                        data(cm)._used = True

                        Dim lookForGroupInternalId As Long = item.linkedToGroupInternalId

                        ' find the ID
                        Dim found = True

                        Do Until found = False
                            Dim qq As Long = 0
                            found = False

                            Dim lower = cm - 200
                            Dim upper = cm + 200
                            If lower < 0 Then lower = 0
                            If upper > data.Length - 1 Then upper = data.Length - 1
                            For ha As Long = lower To upper


                                Dim item2 = data(ha)

                                If item2._used = False Then

                                    If (item2.groupInternalId = lookForGroupInternalId) Then
                                        If item2.codeGroup = item.codeGroup Then



                                            Dim el2 As New shapElementStruct
                                            el2.name = item2.name
                                            el2.id = item.codeGroup

                                            el2.origin = item2.origin
                                            el2.type = item2.type
                                            el2.markingText = item2.markingDescription
                                            el2.validUntil = item2.validUntil
                                            el2.lat = item2.latitude
                                            el2.lon = item2.longitude
                                            el2.description = item2.groupDescription
                                            el2.lighted = item2.lighted
                                            el2.marked = item2.marked
                                            lineF.Add(el2)

                                            data(ha)._used = True
                                            lookForGroupInternalId = item2.linkedToGroupInternalId
                                            usedsItems += 1
                                            found = True

                                            '       Console.Write("-")

                                        End If
                                    End If
                                End If

                                qq += 1
                            Next
                        Loop
                        'Console.Write("T")

                        ' check dist
                        For i As Long = 1 To lineF.Count - 1

                            Dim p1 = lineF(i)
                            Dim p2 = lineF(i - 1)

                            Dim dist = GetGreatCircleDistance_ConstEarthRadiusInNm(p1.lon, p2.lon, p1.lat, p2.lat)
                            If dist > 5 Then
                                Console.WriteLine("WARN: distance between cabels too long! " & dist & "[Nm] " & p1.name)

                            End If

                        Next

                        lines.Add(lineF)
                End Select
            End If
            cm += 1
        Next

    End Sub

    Dim headerline() As String = Nothing
    Structure csvStruct
        Dim codeGroup As String
        Dim groupInternalId As Long
        Dim groupDescription As String
        Dim name As String
        Dim type As String
        Dim lighted As Boolean
        Dim markingDescription As String
        Dim heightUnit As String
        Dim heightValue As Long
        Dim elevationValue As Long
        Dim latitude As Double
        Dim longitude As Double
        Dim defaultHeightFlag As Boolean
        Dim marked As Boolean
        Dim verticalPrecision As Double
        Dim lateralPrecision As Double
        Dim obstacleRadius As Double
        Dim linkedToGroupInternalId As Long
        Dim linkType As String
        Dim validUntil As Date
        Dim effectiveFrom As Date
        Dim origin As String
        Dim _used As Boolean
    End Structure

    Public replFrom As String = "."
    Public replTo As String = ","
    Dim seperator As String = ";"

    Function parseCsv() As csvStruct()

        If decimalSeparator = "." Then
            replFrom = ","
            replTo = "."
        End If


        Dim reader As New StreamReader(csvFile, Encoding.Default)
        Dim afile As FileIO.TextFieldParser = New FileIO.TextFieldParser(csvFile)
        Dim CurrentRecord As String() ' this array will hold each line of data
        afile.TextFieldType = FileIO.FieldType.Delimited
        afile.Delimiters = New String() {";"}
        afile.HasFieldsEnclosedInQuotes = True

        Dim dataLst As New List(Of csvStruct)


        Dim cntrLog As Short = 0
        Dim kCntr As Long = 0

        ' parse the actual file
        Do While Not afile.EndOfData
            Try

                ' header line
                CurrentRecord = afile.ReadFields
                If headerline Is Nothing Then
                    headerline = CurrentRecord
                Else
                    Dim newRow As csvStruct
                    newRow.codeGroup = getValue(CurrentRecord, "codeGroupId")
                    Try
                        If getValue(CurrentRecord, "locGroupMemberId") <> "" Then newRow.groupInternalId = getValue(CurrentRecord, "locGroupMemberId")
                    Catch ex As Exception
                    End Try

                    newRow.groupDescription = getValue(CurrentRecord, "txtGroupName")
                    newRow.name = getValue(CurrentRecord, "txtName")
                    newRow.type = getValue(CurrentRecord, "codeType")

                    newRow.lighted = getValue(CurrentRecord, "codeLgt").ToString.ToUpper = "Y".ToString.ToUpper
                    newRow.markingDescription = getValue(CurrentRecord, "txtDescrMarking")
                    newRow.marked = getValue(CurrentRecord, "codeMarking").ToString.ToLower = "Y".ToLower
                    newRow.heightUnit = getValue(CurrentRecord, "uomDistVer")
                    newRow.heightValue = getValue(CurrentRecord, "valHgt")
                    Try
                        newRow.elevationValue = getValue(CurrentRecord, "valElev").ToString.Replace(replFrom, replTo)
                    Catch ex As Exception
                    End Try

                    newRow.latitude = CType(getValue(CurrentRecord, "geoLat").ToString.Replace(replFrom, replTo), Double)
                    newRow.longitude = getValue(CurrentRecord, "geoLong").ToString.Replace(replFrom, replTo)
                    newRow.defaultHeightFlag = getValue(CurrentRecord, "defaultHeightFlag").ToString.ToUpper = "Y".ToString.ToUpper
                    Try
                        If getValue(CurrentRecord, "codeHgtAccuracy") <> "" Then newRow.verticalPrecision = getValue(CurrentRecord, "codeHgtAccuracy")
                    Catch ex As Exception
                        Console.WriteLine("ERR: cant read: verticalPrecision")
                    End Try

                    Try
                        If getValue(CurrentRecord, "valRadiusAccuracy") <> "" Then newRow.lateralPrecision = getValue(CurrentRecord, "valRadiusAccuracy")
                    Catch ex As Exception
                        Console.WriteLine("ERR: cant read: valRadiusAccuracy")
                    End Try

                    Try
                        If getValue(CurrentRecord, "valRadius") <> "" And getValue(CurrentRecord, "valRadius") <> "bazl" Then newRow.obstacleRadius = getValue(CurrentRecord, "valRadius")
                    Catch ex As Exception
                        Console.WriteLine("ERR: cant read: obstacleRadius")
                    End Try

                    Try
                        If getValue(CurrentRecord, "locLinkedToGroupMemberId") <> "" Then newRow.linkedToGroupInternalId = getValue(CurrentRecord, "locLinkedToGroupMemberId")
                    Catch ex As Exception
                        Console.WriteLine("ERR: cant read: linkedToGroupInternalId")
                    End Try

                    newRow.linkType = getValue(CurrentRecord, "codeLinkType")
                    Try
                        If getValue(CurrentRecord, "datetimeValidTil") <> "" Then newRow.validUntil = getValue(CurrentRecord, "datetimeValidTil")
                        If getValue(CurrentRecord, "datetimeValidWef") <> "" Then newRow.effectiveFrom = getValue(CurrentRecord, "datetimeValidWef")
                    Catch ex As Exception
                        Console.WriteLine("ERR: cant read: linkType")
                    End Try

                    newRow.origin = getValue(CurrentRecord, "source")

                    'If newRow.origin = "openstreetmap" Then
                    '    Dim k = 3
                    'End If

                    dataLst.Add(newRow)

                    ' testwise
                    'If dataLst.Count > 30000 Then Return dataLst.ToArray
                    cntrLog += 1
                    If cntrLog > 1000 Then
                        cntrLog = 0
                        Console.WriteLine(kCntr & "k elements read..")
                        kCntr += 1
                    End If
                End If


            Catch ex As FileIO.MalformedLineException
                Stop
            End Try
        Loop


        Return dataLst.ToArray

    End Function

    Function getValue(CurrentRecord As Object, col As String)

        ' find idx
        Dim fieldFound As Boolean = False
        For i As Short = 0 To headerline.Length - 1
            If headerline(i) = col Then
                fieldFound = True

                Return CurrentRecord(i)

            End If
        Next

        If fieldFound = False Then
            Console.WriteLine("ERR: field not found! " & col)
            Console.ReadKey()
        End If

        Return ""
    End Function


    ' geo functions

    ' Coordinate Transformations
    Dim EarthRadius As Double = 6378.137 / 1.852 ' in nautical Miles


    ' Function TileNbr to Position
    <Serializable()> Structure DoublePointStruct
        Dim x As Double
        Dim y As Double
        Dim rmk As String
    End Structure
    Structure VectorStruct
        Dim x As Double
        Dim y As Double
        Dim z As Double
    End Structure

    Function GetGreatCircleDistance_ConstEarthRadiusInNm(x1 As Double, x2 As Double, y1 As Double, y2 As Double) As Double

        Dim position1 As DoublePointStruct
        Dim position2 As DoublePointStruct

        If x1 = x2 And y1 = y2 Then Return 0

        position1.x = x1
        position2.x = x2
        position1.y = y1
        position2.y = y2
        ' Notes from 22.3.2012
        Dim V1 As VectorStruct
        V1.x = EarthRadius * (Math.Cos(position1.y * Math.PI / 180) * Math.Cos(position1.x * Math.PI / 180))
        V1.y = EarthRadius * (Math.Cos(position1.y * Math.PI / 180) * Math.Sin(position1.x * Math.PI / 180))
        V1.z = EarthRadius * (Math.Sin(position1.y * Math.PI / 180))

        Dim V2 As VectorStruct
        V2.x = EarthRadius * (Math.Cos(position2.y * Math.PI / 180) * Math.Cos(position2.x * Math.PI / 180))
        V2.y = EarthRadius * (Math.Cos(position2.y * Math.PI / 180) * Math.Sin(position2.x * Math.PI / 180))
        V2.z = EarthRadius * (Math.Sin(position2.y * Math.PI / 180))

        Dim AngleBetw As Double = Math.Acos((V1.x * V2.x + V1.y * V2.y + V1.z * V2.z) / (Math.Sqrt(V1.x ^ 2 + V1.y ^ 2 + V1.z ^ 2) * Math.Sqrt(V2.x ^ 2 + V2.y ^ 2 + V2.z ^ 2)))

        ' Return Distance in nautical Miles
        Return AngleBetw * EarthRadius
    End Function


    Function makePolygonConcarve(inPts() As DoublePointStruct) As DoublePointStruct()

        If inPts.Length = 1 Then Return inPts

        Dim minpt As New DoublePointStruct
        For Each p In inPts
            If (p.x < minpt.x And p.y < minpt.y) Or (minpt.x = 0 And minpt.y = 0) Then
                minpt = p
            End If
        Next

        Dim startEndPt As DoublePointStruct = minpt

        Dim retPts As New List(Of DoublePointStruct)
        retPts.Add(startEndPt)

        Dim currentPoint As DoublePointStruct = startEndPt
        Dim acPoint As New DoublePointStruct



        Dim cn As Long = 0
        Dim oldBearing As Double = 0
        Do Until (acPoint.x = startEndPt.x And acPoint.y = startEndPt.y) Or cn > inPts.Length
            Dim minBearing As Double = 0
            cn += 1
            Dim brg = 0
            For Each p In inPts
                If p.x <> currentPoint.x And p.y <> currentPoint.y Then

                    ' get bearing
                    brg = getPointBrgCartesic(currentPoint, p) - oldBearing

                    If brg >= 0 And (brg < minBearing Or minBearing = 0) Then
                        minBearing = brg
                        acPoint = p
                    End If

                End If
            Next
            currentPoint.x = acPoint.x
            currentPoint.y = acPoint.y

            oldBearing = minBearing + oldBearing

            If retPts.Contains(acPoint) = False Then retPts.Add(acPoint)

        Loop


        Return retPts.ToArray
    End Function
    Private Function getPointBrgCartesic(p1 As DoublePointStruct, p2 As DoublePointStruct) As Double

        Dim brg = 90 - Math.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Math.PI
        If brg < 0 Then
            brg += 360
        End If
        If brg > 360 Then
            brg -= 360
        End If

        Return brg
    End Function
    Public Function FindCentroid(ByVal points() As DoublePointStruct) As DoublePointStruct

        Dim x As Single = 0
        Dim y As Single = 0

        If points Is Nothing Then Return Nothing

        For Each p In points
            x += p.x
            y += p.y
        Next

        x /= (points.Length)
        y /= (points.Length)

        Dim ssd = New DoublePointStruct
        ssd.x = x
        ssd.y = y
        Return ssd
    End Function
    Public Function GetRadialCoordinates(ByVal CenterPoint As DoublePointStruct, ByVal Alpha As Double, ByVal Distance_Nm As Double, Optional exact_whichNeedsIterating As Boolean = False) As DoublePointStruct


        ' Use Notes from 21.3.2012
        Dim RetPt As New DoublePointStruct

        If Distance_Nm = 0 Then Return CenterPoint

        'Alpha = -90
        'CenterPoint.x = 13
        'CenterPoint.y = 47
        'Distance_Nm = 60

        If Distance_Nm < 0 Then
            Distance_Nm *= -1
            Alpha += 180
        End If

        Dim westHemisphere As Double = 1

        Do Until Alpha > 0
            Alpha += 360
        Loop

        Do Until Alpha < 360
            Alpha -= 360
        Loop

        If Alpha > 180 Then
            westHemisphere = -1
        End If

        Dim costheta As Double = Math.Cos(Distance_Nm / EarthRadius)
        Dim cosa As Double = Math.Cos(CenterPoint.y * Math.PI / 180)
        Dim cosalpha As Double = Math.Cos(Alpha * Math.PI / 180)

        Dim sintheta As Double = Math.Sin(Distance_Nm / EarthRadius)
        Dim sina As Double = Math.Sin(CenterPoint.y * Math.PI / 180)

        RetPt.y = Math.Asin(cosalpha * cosa * sintheta + sina * costheta) * 180 / Math.PI

        Dim n As Double = (Math.Sqrt(1 - (cosalpha * cosa * sintheta + costheta * sina) ^ 2))
        Dim z As Double = (costheta * cosa - cosalpha * sintheta * sina)

        If z > n Then n = z

        RetPt.x = CenterPoint.x + Math.Acos(z / n) * 180 / Math.PI * westHemisphere


        Return RetPt

        'If Distance_Nm < 0 Then
        '    Distance_Nm *= -1
        '    Alpha += 180
        'End If

        'exact_whichNeedsIterating = 0
        'Select Case exact_whichNeedsIterating
        '    Case True
        '        Dim cntr As Short = 0
        '        Dim Distance_Nm2 As Double
        '        Dim x As Double = 0

        '        Do Until Math.Abs(x - Distance_Nm) < 0.01 Or Distance_Nm < 1 Or cntr > 5


        '            cntr += 1

        '            Dim Beta As Double = Math.Asin(Distance_Nm2 / EarthRadius * Math.Cos(CenterPoint.y * Math.PI / 180) * Math.Cos(-Alpha * Math.PI / 180) + Math.Sin(CenterPoint.y * Math.PI / 180)) * 180 / Math.PI

        '            Dim cosBeta As Double = Math.Cos(CenterPoint.y * Math.PI / 180)
        '            Dim sinBeta As Double = Math.Sin(CenterPoint.y * Math.PI / 180)
        '            Dim cosLambda As Double = Math.Cos(CenterPoint.x * Math.PI / 180)
        '            Dim sinLambda As Double = Math.Sin(CenterPoint.x * Math.PI / 180)
        '            Dim cosAlpha As Double = Math.Cos(Alpha * Math.PI / 180)
        '            Dim sinAlpha As Double = Math.Sin(Alpha * Math.PI / 180)

        '            Dim af As Double = Distance_Nm2 / (EarthRadius * 2 * Math.PI) * Math.PI * 2

        '            If af <> 0 Then
        '                Dim k = 3
        '            End If

        '            Distance_Nm2 = EarthRadius * Math.Sin(af)
        '            Dim V As VectorStruct
        '            V.x = -Distance_Nm2 * cosLambda * sinBeta * cosAlpha - Distance_Nm2 * 1 * sinLambda * sinAlpha + EarthRadius * cosLambda * cosBeta
        '            V.y = -Distance_Nm2 * sinLambda * cosAlpha * sinBeta + Distance_Nm2 * 1 * cosLambda * sinAlpha + EarthRadius * sinLambda * cosBeta


        '            RetPt.x = Math.Atan2(V.y, V.x) * 180 / Math.PI
        '            RetPt.y = Beta

        '            x = GetGreatCircleDistance_ConstEarthRadiusInNm(RetPt, CenterPoint)

        '            If x > Distance_Nm Then
        '                Distance_Nm2 -= 0.01
        '            Else
        '                Distance_Nm2 += 0.01
        '            End If

        '        Loop

        '        Return RetPt
        '    Case False

        '        Dim Beta As Double = Math.Asin(Distance_Nm / EarthRadius * Math.Cos(CenterPoint.y * Math.PI / 180) * Math.Cos(-Alpha * Math.PI / 180) + Math.Sin(CenterPoint.y * Math.PI / 180)) * 180 / Math.PI

        '        Dim cosBeta As Double = Math.Cos(CenterPoint.y * Math.PI / 180)
        '        Dim sinBeta As Double = Math.Sin(CenterPoint.y * Math.PI / 180)
        '        Dim cosLambda As Double = Math.Cos(CenterPoint.x * Math.PI / 180)
        '        Dim sinLambda As Double = Math.Sin(CenterPoint.x * Math.PI / 180)
        '        Dim cosAlpha As Double = Math.Cos(Alpha * Math.PI / 180)
        '        Dim sinAlpha As Double = Math.Sin(Alpha * Math.PI / 180)
        '        Dim r As Double = EarthRadius

        '        Dim af As Double = Distance_Nm / (r * 2 * Math.PI) * Math.PI * 2



        '        Distance_Nm = r * Math.Sin(af)
        '        Dim V As VectorStruct
        '        V.x = -Distance_Nm * cosLambda * sinBeta * cosAlpha - Distance_Nm * 1 * sinLambda * sinAlpha + r * cosLambda * cosBeta
        '        V.y = -Distance_Nm * sinLambda * cosAlpha * sinBeta + Distance_Nm * 1 * cosLambda * sinAlpha + r * sinLambda * cosBeta


        '        RetPt.x = Math.Atan2(V.y, V.x) * 180 / Math.PI
        '        RetPt.y = Beta
        '        Return RetPt
        'End Select

    End Function
End Module
