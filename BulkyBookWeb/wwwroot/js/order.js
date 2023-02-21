var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else {
        if (url.includes("approved")) {
            loadDataTable("approved");
        }
        else {
            if (url.includes("inprocess")) {
                loadDataTable("inprocess");
            }
            else {
                if (url.includes("completed")) {
                    loadDataTable("completed");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status){
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "userName", "width": "15%" },
            { "data": "appUser.email", "width": "20%" },
            { "data": "phoneNumber", "width": "20%" },
            { "data": "orderTotal", "width": "15%" },
            { "data": "orderStatus", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="btn-group">
                                <a href="/Admin/Order/Details?orderId=${data}">
                                    <i class="bi bi-info-circle"></i>
                                </a>
                            </div>
                           `
                },
                "width": "5%"
            }
            ]
        });
}




