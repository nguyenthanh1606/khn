
var directionDisplay;

var directionsService;
var stepDisplay;

var position;
var marker = [];
var polyline = [];
var poly2 = [];
var poly = null;
var startLocation = [];
var endLocation = [];
var timerHandle = [];


var speed = 0.000005, wait = 1;
var infowindow = null;

var myPano;
var panoClient;
var nextPanoId;

var startLoc = new Array();
startLoc[0] = 'rio claro, trinidad';


var endLoc = new Array();
endLoc[0] = 'princes town, trinidad';



var Colors = ["#FF0000", "#00FF00", "#0000FF"];
var listdt;

function initialize_() {

    infowindow = new google.maps.InfoWindow(
      {
          size: new google.maps.Size(150, 50)
      });

    //var myOptions = {
    //    zoom: 16,
    //    mapTypeId: google.maps.MapTypeId.ROADMAP
    //}
    //map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);

    //address = 'Trinidad and Tobago'
    //geocoder = new google.maps.Geocoder();
    //geocoder.geocode({ 'address': address }, function (results, status) {
    //    map.fitBounds(results[0].geometry.viewport);

    //});
    // setRoutes();
}


function createMarker(latlng, label, html) {
    // alert("createMarker("+latlng+","+label+","+html+","+color+")");
    var contentString = '<b>' + label + '</b><br>' + html;
    var marker = new google.maps.Marker({
        position: latlng,
        map: map,
        title: label,
        zIndex: Math.round(latlng.lat() * -100000) << 5
    });
    marker.myname = label;


    google.maps.event.addListener(marker, 'click', function () {
        infowindow.setContent(contentString);
        infowindow.open(map, marker);
    });
    return marker;
}
var index_list = 0;
var marker1;

function animateLotrinh(listLotrinh) {
    //map = null;
   // setupMap(10.85586, 106.67961, 15);
    listdt = listLotrinh;
    index_list = 0
    initialize_();
    marker1 = new google.maps.Marker({
        position: listdt[index_list],
        map: map,
        title: '',
        zIndex: Math.round(listdt[index_list] * -100000) << 5
    });
    
    setRoutes(index_list);

}
var indexDisp = -1;
function setRoutes(x) {
    indexDisp += 1;
    //alert("setRoutes: " + indexDisp);
    if (state_animate == 1) {
        if (x == listdt.length - 1) {
            var runbtn_ = document.getElementById('runbtn');           
            runbtn_.value = 'Dừng lại';
            if (marker1 != null) {
                marker1.setMap(null);
            }
            return;
        }
        var datesave = new Date(parseInt(listdata[x].DateSave.replace('/Date(', '').replace(')', '')));
        var time = datesave.getDate() + '-' + (datesave.getMonth() + 1) + '-' + datesave.getFullYear() + ' ' + datesave.getHours() + ':' + datesave.getMinutes() + ':' + datesave.getSeconds();
        var txttime = document.getElementById('txttime');
        txttime.value = time;
        var txtcoors = document.getElementById('txtcoors');
        txtcoors.value = listdata[x].Latitude + ',' + listdata[x].Longitude;
        var txtadd = document.getElementById('txtadd');
        txtadd.value = listdata[x].Addr;
        var txtspeed = document.getElementById('txtspeed');
        txtspeed.value = listdata[x].Speed + " km/h";
        // alert(listdt[x].lat()+" "+ listdt[x].lng());
        startLoc = new Array();

        endLoc = new Array();

        startLoc[0] = listdt[x];

        endLoc[0] = listdt[x + 1];
         

        for (var i = 0; i < startLoc.length; i++) {

            var rendererOptions = {
                map: map,
                suppressMarkers: true,
                preserveViewport: true
            }
            directionsService = new google.maps.DirectionsService();

            var travelMode = google.maps.DirectionsTravelMode.DRIVING;

            var request = {
                origin: startLoc[i],
                destination: endLoc[i],
                travelMode: travelMode
            };
           
            directionsDisplay[indexDisp] = new google.maps.DirectionsRenderer(rendererOptions);
           // alert(indexDisp);
           // alert("directionsDisplay: " + directionsDisplay.length);
            directionsService.route(request, makeRouteCallback(i, directionsDisplay[indexDisp]));

        }


        function makeRouteCallback(routeNum, disp) {
            //if (polyline[routeNum] && (polyline[routeNum].getMap() != null)) {
            //    startAnimation(routeNum);
            //    return;
            //}
            return function (response, status) {

                if (status == google.maps.DirectionsStatus.OK) {

                    var bounds = new google.maps.LatLngBounds();
                    var route = response.routes[0];
                    startLocation[routeNum] = new Object();
                    endLocation[routeNum] = new Object();


                    polyline[routeNum] = new google.maps.Polyline({
                        path: [],
                        strokeColor: '#FF0000',
                        strokeOpacity: 0.00001,
                        strokeWeight: 0
                    });

                    poly2[routeNum] = new google.maps.Polyline({
                        path: [],
                        strokeColor: '#FF0000',
                        strokeOpacity: 0.00001,
                        strokeWeight: 0
                    });


                    // For each route, display summary information.
                    var path = response.routes[0].overview_path;
                    var legs = response.routes[0].legs;


                   // disp = new google.maps.DirectionsRenderer(rendererOptions);
                    disp.setMap(map);
                    disp.setDirections(response);


                    //Markers               
                    for (i = 0; i < legs.length; i++) {
                        if (i == 0) {
                            startLocation[routeNum].latlng = legs[i].start_location;
                            startLocation[routeNum].address = legs[i].start_address;
                            // marker = google.maps.Marker({map:map,position: startLocation.latlng});
                            //marker[routeNum] = createMarker(legs[i].start_location, "start", legs[i].start_address, "green");
                            marker1.setPosition(legs[i].start_location);
                            marker[routeNum] = marker1;
                        }
                        endLocation[routeNum].latlng = legs[i].end_location;
                        endLocation[routeNum].address = legs[i].end_address;
                        var steps = legs[i].steps;

                        for (j = 0; j < steps.length; j++) {
                            var nextSegment = steps[j].path;
                            var nextSegment = steps[j].path;

                            for (k = 0; k < nextSegment.length; k++) {
                                polyline[routeNum].getPath().push(nextSegment[k]);
                                //bounds.extend(nextSegment[k]);
                            }

                        }
                    }

                }
               // console.log(routeNum);
                polyline[routeNum].setMap(map);

                //map.fitBounds(bounds);
                startAnimation(routeNum);

            } // else alert("Directions request failed: "+status);

        }
    }
    else {
        stop_ = 1;
    }
   

}

var lastVertex = 1;
var stepnum = 0;
var step = 100; // 5; // metres
var tick = 100; // milliseconds
var eol = [];
//----------------------------------------------------------------------                
function updatePoly(i, d) {
    // Spawn a new polyline every 20 vertices, because updating a 100-vertex poly is too slow
    if (poly2[i].getPath().getLength() > 20) {
        poly2[i] = new google.maps.Polyline([polyline[i].getPath().getAt(lastVertex - 1)]);
        // map.addOverlay(poly2)
    }

    if (polyline[i].GetIndexAtDistance(d) < lastVertex + 2) {
        if (poly2[i].getPath().getLength() > 1) {
            poly2[i].getPath().removeAt(poly2[i].getPath().getLength() - 1)
        }
        poly2[i].getPath().insertAt(poly2[i].getPath().getLength(), polyline[i].GetPointAtDistance(d));
    } else {
        poly2[i].getPath().insertAt(poly2[i].getPath().getLength(), endLocation[i].latlng);
    }
}
//----------------------------------------------------------------------------

function animate(index, d) {
    if (d > eol[index]) {
        
        marker[index].setPosition(endLocation[index].latlng);
        //alert(index_list);
        index_list+=1;
        setRoutes(index_list);
        return;
    }
    var p = polyline[index].GetPointAtDistance(d);

    map.panTo(p);
    marker[index].setPosition(p);
   // map.pan
    updatePoly(index, d);
    timerHandle[index] = setTimeout("animate(" + index + "," + (d + step) + ")", tick);
}

//-------------------------------------------------------------------------

function startAnimation(index) {
    if (timerHandle[index]) clearTimeout(timerHandle[index]);
    eol[index] = polyline[index].Distance();
    //map.setCenter(polyline[index].getPath().getAt(0));

    poly2[index] = new google.maps.Polyline({ path: [polyline[index].getPath().getAt(0)], strokeColor: "#FFFF00", strokeWeight: 3 });

    timerHandle[index] = setTimeout("animate(" + index + ",50)", 500);  // Allow time for the initial map display

}

//----------------------------------------------------------------------------    

