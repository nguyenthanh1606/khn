
window.addEventListener('DOMContentLoaded', (event) => {
    var list_sidebar = document.querySelectorAll('.header-content ul li .header-content-head .item_txt');
});

/* set dropdown menu for sidebar */
var list_sidebar = document.querySelectorAll('.header-content ul li .header-content-head .item_txt');
Array.from(list_sidebar).forEach(sidebar => {
    sidebar.addEventListener('click', () => {
        var sidebar_parent = sidebar.parentElement.parentElement;
        var sidebar_content = sidebar_parent.querySelector('.dropdown_item');
        var sidebar_active = document.querySelector('.header-content ul li .dropdown_item.active');
        
        if (sidebar_active) {
            sidebar_content === sidebar_active ? sidebar_content.classList.remove('active') : (sidebar_active.classList.remove('active'), sidebar_content.classList.add('active'));
        } else {
            sidebar_content.classList.add('active')
        }
    })
});

/* open sidebar for mobile */
var isOpen = false;
var side_bar = document.querySelector('.open_sidebar');
var list_item = document.querySelectorAll('.header-content ul li .header-content-head');

side_bar ? side_bar.addEventListener('click', openSideBar) : '';

function openSideBar(){
    Array.from(list_item).forEach(item => {
        var item_txt = item.querySelector('.item_txt');
        var icon_open = document.querySelector('.header__ .open_sidebar i');
        var header__ = document.querySelector(".header__");
        var dropdown_active = document.querySelector('.dropdown_item.active');
        dropdown_active ? dropdown_active.classList.remove("active") : '';

        !isOpen ? (item.classList.add('active'), side_bar.classList.add('active'), header__.classList.add('active'), item_txt.classList.add('active'), icon_open.classList.remove('fa-angle-double-right'), icon_open.classList.add('fa-angle-double-left')) : (side_bar.classList.add('active'), item_txt.classList.remove('active'), icon_open.classList.add('fa-angle-double-right'), icon_open.classList.remove('fa-angle-double-left'), header__.classList.remove('active'), item.classList.remove('active'));
    })
    isOpen = !isOpen;
}

var list_item_status = document.querySelectorAll('.content-status_item');
console.log(list_item_status);
Array.from(list_item_status).forEach((item) => {
    item.addEventListener('click', function () {
        var itemActive = document.querySelector('.wrapper_content.active');
        itemActive ? itemActive.classList.remove('active') : '';
        var setItemActive = item.querySelector('.wrapper_content');
        setItemActive.classList.add('active');
    })
})
