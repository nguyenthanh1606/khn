window.onscroll = function () {
    var navbar = document.querySelector('.ui.fixed');
    if (document.body.scrollTop > 55) {
        navbar.style.position = 'fixed';
    } else {
        navbar.style.position = 'sticky';
    }

}

window.addEventListener("keypress", function (e) {
    if (e.keyCode === 13) {
        SubmitForm();
    }
})

window.onload = function () {
    var array_imgBG = ["../../images/bg/bg_01.svg", "../../images/bg/bg_02.svg", "../../images/bg/bg_03.svg"];
    var screenHeight = window.innerHeight;
    if (screenHeight <= 764) {
        var setImg = document.querySelector('.login_content_left_img img');
        setImg ? setImg.style.width = (0.6 * screenHeight) + 'px' : '';
    }

    //var bg = document.querySelector('.bg');
    //var index = 0;
    //console.log(bg);
    //setInterval(function () {
    //    bg.classList.add(`bg_${index}`);
    //    index > 2 ? index = 0 : index++;
    //}, 3000);
}

window.onresize = function () {
    var screenHeight = window.innerHeight;
    if (screenHeight <= 764) {
        var setImg = document.querySelector('.login_content_left_img img');
        setImg ? setImg.style.width = (0.6 * screenHeight) + 'px' : ' ';
    }
}


/** tao read more text cho thong bao*/
var list_btn_read_more = document.querySelectorAll('.read_more-btn');
Array.from(list_btn_read_more).forEach(btn => {
    btn.addEventListener('click', () => {
        var parent = btn.parentNode;
        var read_more_text = parent.querySelector('.read_more-text');
        read_more_text.style.display = "inline";
        btn.style.display = "none";
    })
})

/** show noti conent khi click vao chuong thong bao*/
var show = false;
var bell = document.querySelector('.right.menu');
bell ? bell.addEventListener('click', showNofi) : '';
function showNofi() {
    var noti = document.querySelector('.noti-wrapper');
    !show ? (noti.classList.add('active'), show = true) : (noti.classList.remove('active'), show = false);

    var read_more_button = document.querySelectorAll('.read_more-btn');
    var read_more_text = document.querySelectorAll('.read_more-text');
    Array.from(read_more_button).forEach(btn => {
        btn.style.display = 'inline-block';
    })

    Array.from(read_more_text).forEach(text => {
        text.style.display = 'none';
    })
}

var checkTogle = false;
function toggle() {

    var ui_content_left = document.querySelector('.ui-content_left');
    var icon = document.querySelector('.tab_bars i');
    if (!checkTogle) {
        checkTogle = true;
        ui_content_left.classList.add('active');
        icon.classList.remove('fa-bars');
        icon.classList.add('fa-times');
    } else {
        checkTogle = false;
        ui_content_left.classList.remove('active');
        icon.classList.add('fa-bars');
        icon.classList.remove('fa-times');
    }
    //check ? (check = false, ui_content_left.classList.add('active')) : (check = true, ui_content_left.classList.remove('active'));
}

/** create notification error when not exist user */
function showError() {
    var noti_error = document.querySelector('.noti_error');
    noti_error.classList.add('active');

    setInterval(function () {
        noti_error.classList.remove('active');
    }, 6000);

}



/** show tab payment by data-id */
var list_tab = document.querySelectorAll('.payment-tab p');
Array.from(list_tab).forEach(tab => {
    tab.addEventListener('click', () => {
        var index = tab.getAttribute('data-id');
        var tab_active = document.querySelector('.payment-tab p.active');
        var tab_active_index = tab_active.getAttribute('data-id');
        var tab_content_active = document.querySelector('.payment-tab-content_txt.active');

        if (index !== tab_active_index) {
            var set_tab_active = document.querySelector(`.payment-tab p[data-id="${index}"]`);
            var set_tab_content_active = document.querySelector(`.payment-tab-content-${index}`);

            tab_active.classList.remove('active');
            set_tab_active.classList.add('active');

            tab_content_active.classList.remove('active');
            set_tab_content_active.classList.add('active');
        }
    })
})




