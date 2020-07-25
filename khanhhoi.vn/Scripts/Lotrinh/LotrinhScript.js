function LoadLotrinh() {
    var vehicleidrSelect = document.getElementById('list_xelotrinh').options[document.getElementById('list_xelotrinh').selectedIndex].value;
    var dateform = document.getElementById("date_form_d").value + "T" + document.getElementById("date_form_h").value + "00";
    var dateto = document.getElementById("date_t_d").value + "T" + document.getElementById("date_t_h").value + "00";
    //alert(vehicleNumverSelect);
    $.ajax({
        type: 'POST',
        dataType: "json",
        url: '@(Url.Action("Report", "Home"))',
        data: { indexReport: 1, DeviceID: vehicleidrSelect, dateform: dateform, dateto: dateto },
        success: function (data, txtStatus, XMLHttpRequest) {
            if (data == '0001') {
                //Het Phien Dang Nhap
                location.href = "Logout";
            }
            else if (data == "") {
                
            }
            else {
                if (data.length > 0) {

                   

                }
            }

        }
    });

};
function setDate() {
    //date_form
    var currentdate = new Date();
    var curentMonth = (currentdate.getMonth() + 1) < 10 ? "0" + (currentdate.getMonth() + 1) : (currentdate.getMonth() + 1);
    var curentdate = currentdate.getDate() < 10 ? "0" + currentdate.getDate() : currentdate.getDate();
    var curentHour = currentdate.getHours() < 10 ? "0" + currentdate.getHours() : currentdate.getHours();
    var curentMinute = currentdate.getMinutes() < 10 ? "0" + currentdate.getMinutes() : currentdate.getMinutes();
    var curentSecond = currentdate.getSeconds() < 10 ? "0" + currentdate.getSeconds() : currentdate.getSeconds();

    // var datetimeF = currentdate.getFullYear() + "-" + curentMonth + "-" + curentdate + "T00:" + "00:00";
    var datetimeF = currentdate.getFullYear() + "-" + curentMonth + "-" + curentdate;
   // var datetimeE = currentdate.getFullYear() + "-" + curentMonth + "-" + curentdate + "T" + curentHour + ":" + curentMinute + ":" + curentSecond;

    document.getElementById("date_form_h").value = "00:00";
    document.getElementById("date_form_d").value = datetimeF;
    //date_t_h
    document.getElementById("date_t_h").value = "23:59";
    document.getElementById("date_t_d").value = datetimeF;
    //alert("asdasdasd");
}