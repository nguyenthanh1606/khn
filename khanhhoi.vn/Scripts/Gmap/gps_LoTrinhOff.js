var lotrinhpoint = [];
var arrayMarker = [];
var infowindows = [];
var infowindowcop = [];
var labelArrayLT = [];
var poly = null;
var poly2 = null;
var lotrinhline = null;
var eol;
var offsetId;
var bounds2 = null;
var lineSymbol = {
    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
    strokeColor: '#393',
    strokeOpacity: 1.0,
    strokeWeight: 0.7,
    fillColor: 'green',
    fillOpacity: 0.9,
    scale: 6
};



function ClearMap() {
    

    //lstIMEI = [];

    infowindows = [];
    if (poly) poly.setMap(null);
    //if (map) map.clearMarkers();
    //if (markerArray.length > 0) {
    //    for (var j = 0; j < markerArray.length; j++) {
    //        markerArray[j].setMap(null);
    //    }
    //}labelArrayLT[count]
    if (window.arrayMarker.length > 0) {
        for (var k = 0; k < window.arrayMarker.length; k++) {
            window.arrayMarker[k].setMap(null);
        }
    }
    if (window.labelArrayLT.length > 0) {
        for (var k = 1; k < window.labelArrayLT.length; k++) {
            window.labelArrayLT[k].setMap(null);
        }
    }


    //markerArray = [];
    window.arrayMarker = [];


    //moi dong draw
    //if (drawingManager != null && drawingManager != undefined && drawingManager != "undefined")
    //    drawingManager.setMap(null);
    //drawingManager = new google.maps.drawing.DrawingManager({
    //    //   drawingMode: google.maps.drawing.OverlayType.MARKER,
    //    drawingControl: true,
    //    drawingControlOptions: {
    //        position: google.maps.ControlPosition.TOP_CENTER,
    //        drawingModes: [
    //            google.maps.drawing.OverlayType.POLYGON]
    //    },
    //    circleOptions: {
    //        fillColor: '#CC99CC',
    //        fillOpacity: 0.2,
    //        strokeWeight: 2,
    //        clickable: false,
    //        editable: true,
    //        zIndex: -1
    //    }
    //});
    //drawingManager.setMap(_map);
    //google.maps.event.addDomListener(drawingManager, 'polygoncomplete', function (polygon) {
    //    //This line make it possible to edit polygon you have drawed.
    //    polygon.setEditable(true);

    //    //Call function to pass polygon as parameter to save its coordinated to an array.
    //    save_coordinates_to_array(polygon);

    //    //This event is inside 'polygoncomplete' and fires when you edit the polygon by moving one of its anchors.
    //    google.maps.event.addListener(polygon.getPath(), 'set_at', function () {
    //        alert('changed');
    //        save_coordinates_to_array(polygon);
    //    });

    //    //This event is inside 'polygoncomplete' too and fires when you edit the polygon by moving on one of its anchors.
    //    google.maps.event.addListener(polygon.getPath(), 'insert_at', function () {
    //        alert('also changed');
    //        save_coordinates_to_array(polygon);
    //    });
    //});

    //het dong

    //if (markerCluster != null && markerCluster != "undefined" && markerCluster != "") {

    //    markerCluster.clearMarkers();
    //}
    console.log('clear');
}
function LoadLoTrinh(listdata) {
   
    if (listdata == null) {
        alert("Không có dữ liệu");
        return;
    }
    else {
        if (listdata.length==0) {
            alert("Không có dữ liệu");
            return;
        }
    }
    lotrinhpoint = [];
    var count = 0;
    var bounds = new google.maps.LatLngBounds();
    //if (device.get('isActive') == "false") {
    //    Ext.Msg.alert("Thông báo", "Thiết bị đã hết hạn");
    //    return;
    //}
    ClearMap();
    for (var i = 0; i < listdata.length; i++) {
        count = count + 1;
        var point = new google.maps.LatLng(listdata[i].Latitude, listdata[i].Longitude);
        lotrinhpoint.push(point);
        bounds.extend(point);
        var deviceID = listdata[i].DeviceID;
        var imei = listdata[i].Imei;
        var vehiclenumber = listdata[i].VehicleNumber;
        var address = listdata[i].Addr;
        //if (address == 'chưa xác định') {

        //    address = findAddress(point);
        //    console.log('final: ' + address);
        //}
        var datesave = new Date(parseInt(listdata[i].DateSave.replace('/Date(', '').replace(')', '')));
        var time = datesave.getDate()+'-'+(datesave.getMonth()+1)+'-'+datesave.getFullYear()+' '+datesave.getHours()+':'+datesave.getMinutes()+':'+datesave.getSeconds();
        var speed = listdata[i].Speed;
        var latitude = listdata[i].Latitude;
        var logitude = listdata[i].Longitude;
        var statusdoor;
        var statuskey;
        if (listdata[i].Door == 1) {
            if (listdata[i].Switch_Door == 1)
                statusdoor = listdata[i].StatusDoor == 0 ? 1 : 0;
            else
                statusdoor = listdata[i].StatusDoor;
        } else {
            statusdoor = 0;
        }
        if (listdata[i].Key_ == 1) {
            if (listdata[i].Switch_ == 1)
                statuskey = listdata[i].StatusKey == 0 ? 1 : 0;
            else
                statuskey = listdata[i].StatusKey;
        } else {
            statuskey = 1;
        }
        var TypeTransportID = listdata[i].TypeTransportID;
        var ProvinceName = listdata[i].ProvinceName;
        var Business = listdata[i].Business;
        var Chassis = listdata[i].Chassis;
        var Cooler = '</br><b class="columnLeft">Máy lạnh: </b>';
        if (listdata[i].Cooler_ == 1)
            Cooler += listdata[i].Cooler == "0" || listdata[i].Cooler == null ? "Tắt" : "Mở";
        else Cooler = '';
        var Grosston = listdata[i].Grosston;

        var color = listdata[i].color;
        if (color == 'green')
            color = 'blue';
        var status = listdata[i].Status;
        var VehicleCategoryID = listdata[i].VehicleCategoryID;
        var in_out = listdata[i].in_out;
        var x = listdata[i].DLng;
        var y = listdata[i].DLat;

        var QCVN = listdata[i].QCVN;
        var clsInfo = "infowd";
        if (speed == 0) {
            status = '<b>' + (statuskey == 0 ? 'Đỗ' : 'Dừng') + '</b><br>';
        } else if (speed < 80)
            status = 'Tốc độ:<b> ' + speed + 'km/h</b><br>';
        else
            status = 'Tốc độ:<b> ' + speed + 'km/h (Quá tốc độ quy định 80km/h)</b><br>';

        strStatus = '<div class="columnLeft"><b style="float:left;">Tên thiết bị:</b></div><div class="columnRight">&nbsp' + vehiclenumber + '</div>'
            + '<div class="clear"></div>'
            + '<div class="columnLeft"><b style="float:left;">Thời gian: </b></div><div class="columnRight">&nbsp' + time + '</div>' +
            '<div class="clear"></div>';
        if (speed == 0)
            strStatus += '<div class="columnLeft"><b style="float:left;">Trạng thái: </b></div><div class="columnRight">&nbsp' + status + '</div>' +
                '<div class="clear"></div>';
        else {
            strStatus += '<div class="columnLeft"><b style="float:left;">Tốc độ: </b></div><div class="columnRight">&nbsp' + Math.round(Number(speed)) + 'km/h</div>' +
                '<div class="clear"></div>';
        }
        strStatus += '<div class="columnLeft"><b style="float:left;">Tọa độ: </b></div><div class="columnRight">&nbsp' + latitude + ' - ' + logitude + '</div><div class="clear"></div>'
            + '<div class="columnLeft"><b style="float:left;">Vị trí: </b></div><div class="columnRight">&nbsp' + address + '</div>' +
            '<div class="clear"></div>';
        strStatus += '<div class="columnLeft"><b style="float:left;">Cửa: </b></div><div class="columRight">&nbsp' + (statusdoor == 1 ? ' Mở' : ' Đóng') + '</div>'
            + '<div class="columLeft"><b style="float:left;">Khóa: </b></div><div class="columRight">&nbsp' + (statuskey == 1 ? ' Mở' : ' Tắt') + '</div>'
            //     + '</br><b class="columnLeft">Thẻ nhớ: </b>' + in_out + ''
            //  + '</br><b class="columnLeft">Máy lạnh: </b>' + Cooler + ''
            + Cooler + '';

        ///get bo giao thong
        //strStatus += getMoreData(imei);
        //strStatus += '<a onclick=ShowDriver(' + deviceID + ') ><b id="tx">Bấm vào để xem tài xế</b></a>' +
        //    '<div id=\'taixe\'></div>';
        var NameDriver = listdata[i].NameDriver = 'không xác định' ? '' : listdata[i].NameDriver;
        var PhoneDriver = listdata[i].PhoneDriver = 'không xác định' ? '' : listdata[i].PhoneDriver;
        var DriverLicense = listdata[i].DriverLicense = 'không xác định' ? '' : listdata[i].DriverLicense;

        var DayCreateLicense = listdata[i].DriverLicense = 'không xác định' ? '' : listdata[i].DayCreateLicense;
        var DayExpiredLicense = listdata[i].DriverLicense = 'không xác định' ? '' : listdata[i].DayExpiredLicense;
        //var datesave = new Date(parseInt(listdata[i].DateSave.replace('/Date(', '').replace(')', '')));
        //var time = datesave.getDate() + '-' + (datesave.getMonth() + 1) + '-' + datesave.getFullYear() + ' ' + datesave.getHours() + ':' + datesave.getMinutes() + ':' + datesave.getSeconds();
        if (DayCreateLicense != '') {
            var dateDayCreateLicense = new Date(parseInt(listdata[i].DayCreateLicense.replace('/Date(', '').replace(')', '')));
            var timeDayCreateLicense = dateDayCreateLicense.getDate() + '-' + (dateDayCreateLicense.getMonth() + 1) + '-' + dateDayCreateLicense.getFullYear();
        }
        if (DayExpiredLicense != '') {
            var dateDayExpiredLicense = new Date(parseInt(listdata[i].DayExpiredLicense.replace('/Date(', '').replace(')', '')));
            var timeDayExpiredLicense = dateDayExpiredLicense.getDate() + '-' + (dateDayExpiredLicense.getMonth() + 1) + '-' + dateDayExpiredLicense.getFullYear();
        }
        strStatus += '<div style="border-bottom: solid 1px royalblue; padding:2px; "></div>';
        strStatus += '<div class="columnLeft"><b>Tên lái xe: </b></div><div id="lpnTen" class="columnRight">' + NameDriver + '</div><div class="clear"></div>' +
            '<div class="columnLeft"><b>Số điện thoại: </b></div><div id="lpnSdt" class="columnRight">' + PhoneDriver + '</div><div class="clear"></div>' +
            //   '<div class="columnLeft"><b>CMND: </b></div><div id="lpnCmnd" class="columnRight"></div><div class="clear"></div>' +
            '<div class="columnLeft"><b>GPLX: </b></div><div id="lpnGplx" class="columnRight">' + DriverLicense + '</div><div class="clear"></div>' +
            '<div class="columnLeft"><b>Ngày cấp: </b></div><div id="lpnNgaycap" class="columnRight">' + DayCreateLicense + '</div><div class="clear"></div>' +
            '<div class="columnLeft"><b>Ngày hết hạn: </b></div><div id="lpnNgayhethan" class="columnRight">' + DayExpiredLicense + '</div><div class="clear"></div>';
        var markerLB = new MarkerWithLabel({
            position: point,
            // position: point,
            labelContent: (count).toString(),
            title: (count).toString(),
            map: map,
            //labelAnchor: new window.google.maps.Point(22, 0),
            labelClass: "markerlabels", // the CSS class for the label
            labelStyle: { opacity: 0.7 }
        });

        //label.bindTo('text', marker, 'position');
        //if (count == 1) {
        //    markerLB.setIcon("http://www.google.com/mapfiles/dd-start.png");
        //}
        if (count == listdata.length) {
            markerLB.setIcon("http://www.google.com/mapfiles/dd-end.png");
        }
        else {
            if (listdata.length > 500) {
                if (count % 5 == 0) {
                    var point_2 = new google.maps.LatLng(listdata[i].Latitude, listdata[i].Longitude);
                    markerLB.setIcon(create_Icon(point, point_2));
                    if (speed == '0') {
                        var icon_stop = new google.maps.MarkerImage("http://maps.google.com/mapfiles/kml/pal4/icon24.png", new google.maps.Size(24, 24), null, new google.maps.Point(18, 18));
                        markerLB.setIcon(icon_stop);
                    }
                } else {
                    markerLB.setIcon("/Content/images/icon/point_icon.png");
                    markerLB.setVisible(false);
                }
            } else {
                var point_2 = new google.maps.LatLng(listdata[i].Latitude, listdata[i].Longitude);
                markerLB.setIcon(create_Icon(point, point_2));
                if (speed == '0') {
                    var icon_stop = new google.maps.MarkerImage("http://maps.google.com/mapfiles/kml/pal4/icon24.png", new google.maps.Size(24, 24), null, new google.maps.Point(18, 18));
                    markerLB.setIcon(icon_stop);
                }
            }

        }
        markerLB.setMap(map);
        attachInforwindows(markerLB, strStatus);


    }

    var lineSymbol2 = {
        path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
        strokeColor: '#393',
        strokeOpacity: 1.0,
        strokeWeight: 0.7,
        fillColor: 'green',
        fillOpacity: 0.9,
        scale: 6
    };
    poly = new window.google.maps.Polyline({
        path: lotrinhpoint,
        strokeColor: "black",
        strokeOpacity: 0.7,
        strokeWeight: 0.7,
        icons: [{ icon: lineSymbol, offset: '0px' }],
        _map: map
    });
    poly2 = new window.google.maps.Polyline({
        path: lotrinhpoint,
        strokeColor: "black",
        strokeOpacity: 0.7,
        strokeWeight: 2,
        icons: [{ icon: lineSymbol2, offset: '0px' }],
        _map: map
    });
    poly.setMap(map);
    bounds2 = bounds;
    map.fitBounds(bounds);
    map.setZoom(14);
    var point_ = new google.maps.LatLng(listdata[Math.round(listdata.length / 2)].Latitude, listdata[Math.round(listdata.length / 2)].Longitude);
    map.setCenter(point_);
    //map.panTo(point_);
}
function closeInfoWindows() {

    // console.log('Length info: ' + infowindows.length);
    for (var i = 0; i < infowindows.length; i++) {
        infowindows[i].close();
    }
    //    console.log('Length info: ' + infowindowcop.length);
    for (var i = 0; i < infowindowcop.length; i++) {
        infowindowcop[i].close();
    }

}
function attachInforwindows(marker, stringhtml) {
    var infowindow = new google.maps.InfoWindow({
        content: '<input value="X" type=button name=Close onClick="closeInfoWindows();" style="-moz-border-radius: 100px; -webkit-border-radius: 100px; border-radius: 100px;background-color:indianred;float:right;"></div>' + '<div class="infowdLoTrinh" style="background-color:white;">' + stringhtml + '</div>'
    });
    arrayMarker.push(marker);
    infowindows.push(infowindow);
    //infowindow.open(this.map, marker);
    google.maps.event.addListener(marker, 'click', function (event) {
        closeInfoWindows(); //Đóng các Infowindows đang mở
        infowindow.open(map, marker);
    });
    google.maps.event.addListener(infowindow, 'domready', function () {

        // Reference to the DIV which receives the contents of the infowindow using jQuery
        var iwOuter = $('.gm-style-iw');

        /* The DIV we want to change is above the .gm-style-iw DIV.
         * So, we use jQuery and create a iwBackground variable,
         * and took advantage of the existing reference to .gm-style-iw for the previous DIV with .prev().
         */
        var iwBackground = iwOuter.prev();

        //// Remove the background shadow DIV
        iwBackground.children(':nth-child(2)').css({ 'display': 'none' });

        //// Remove the white background DIV
        iwBackground.children(':nth-child(4)').css({ 'display': 'none' });
        // iwOuter.parent().parent().css({ left: '80px' });
        iwBackground.children(':nth-child(1)').attr('style', function (i, s) { return s + 'left: 16px !important;' });

        //// Moves the arrow 76px to the left margin 
        //iwBackground.children(':nth-child(3)').attr('style', function (i, s) { return s + 'left: 76px !important;' });
        iwBackground.children(':nth-child(3)').find('div').children().css({ 'box-shadow': 'rgba(72, 181, 233, 0.6) 0px 1px 6px', 'z-index': '1' });
        var iwCloseBtn = iwOuter.next();

        // Apply the desired effect to the close button
        iwCloseBtn.css({
            height: '15px',
            width: '15px',
            top: '18px',
            left: '353px',
            opacity: '0.7', // by default the close button has an opacity of 0.7
            //   right: '8px', top: '3px', // button repositioning
            border: '1px solid #48b5e9', // increasing button border and new color
            'border-radius': '13px', // circular effect
            //'box-shadow': '0 0 5px #3990B9' // 3D effect to highlight the button
        });

        // The API automatically applies 0.7 opacity to the button after the mouseout event.
        // This function reverses this event to the desired value.
        iwCloseBtn.mouseout(function () {
            $(this).css({ opacity: '1' });
        });
    });
}
function create_Icon(p1, p2) {
    // Compute the bearing of the line in degrees
    var dir = google.maps.geometry.spherical.computeHeading(p1, p2).toFixed(1);
    // round it to a multiple of 3 and correct unusable numbers
    dir = Math.round(dir / 3) * 3;
    if (dir < 0) dir += 240;
    if (dir > 117) dir -= 120;
    var icon = new google.maps.MarkerImage("http://www.google.com/mapfiles/" + "dir_" + dir + ".png", new google.maps.Size(24, 24), null, new google.maps.Point(12, 12));
    return icon;
}
var dem = 0;
function animateCircle() {
   // ClearMap();
    poly2.setMap(map);
    map.fitBounds(bounds2);
    var step = 1000;
    // console.log('poly: ' + poly);
    eol = poly2.Distance();
    map.panTo(poly2.getPath().getAt(0));
    if (lotrinhpoint.length < 100) step = 100;
    else if (lotrinhpoint.length < 300) step = 400;
    else if (lotrinhpoint.length < 500) step = 700;
    else if (lotrinhpoint.length > 500) step = 1000;
    clearInterval(offsetId);
    var count = 0;
    offsetId = window.setInterval(function () {
        dem++;
        count = (count + step);
        var p = poly2.GetPointAtDistance(count);

        var icons = poly2.get('icons');
        icons[0].offset = (count) / eol * 100 + '%';
        poly2.set('icons', icons);
        if ((count) / eol >= 0.997) clearInterval(offsetId);
        //   _map.panTo(p);
        if (dem == 15) {
            map.panTo(p);
            dem = 0;
        }
    }, 200);
}

