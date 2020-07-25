$(document).ready(function () {
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    $('#sidebarCollapse-1').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    $('#sidebarCollapse-2').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    $('#sidebarCollapse-3').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    $('#sidebarCollapse-4').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
    $('#sidebarCollapse-5').on('click', function () {
        $('#sidebar').toggleClass('active');
    });
});
$('[data-toggle="tooltip"]').tooltip();
//$('.sidebarBtn').click(function () {
//    //$('.sidebar').toggleClass('active');
//    //$('.sidebarBtn').toggleClass('toggle');

//    $('.sidebar').toggleClass('active');
//    var _giamsattoggle = document.getElementById("giamsat_toggle");
//    _giamsattoggle.style.display = "none";
//    _giamsattoggle.classList.remove("toggle");

//});

$(document).ready(function () {
    $("#Config_data").click(function () {
        $("#list_Config_data").slideToggle();
    });
});

$('#MeaningOfIcon').click(function () {
    $('#popUpBox_theMeaningOfIcon').fadeIn(200);
});
$('#closeMeaningOfIcon').click(function () {
    $('#popUpBox_theMeaningOfIcon').fadeOut(200);
});

$("#btnConfigVeihicleID").click(function () {
    $('#divListConfigViewRoute').fadeIn(200);
});
$('#btnClose').click(function () {
    $('#divListConfigViewRoute').fadeOut(200);
});
$(".toggle-password").click(function () {
    $(this).toggleClass("fa-eye fa-eye-slash");
    var input = $($(this).attr("toggle"));
    if (input.attr("type") == "password") {
        input.attr("type", "text");
    } else {
        input.attr("type", "password");
    }
});

//$(function () {
//    $('#sub-menu a').click(function () {
//        $('#sub-menu a').removeClass('active');
//        $(this).addClass('active');
//    });
//});