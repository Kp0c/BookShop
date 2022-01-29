let dataTable;

$(document).ready(function () {
    const status = new URL(window.location).searchParams.get('status');
    
    if (status) {
        loadDataTable(status);
    } else {
        loadDataTable('all');
    }
})

function loadDataTable(status) {
    dataTable = $('#ordersData').DataTable({
        ajax: {
            url:`/Admin/Order/GetAll?status=${status}`
        },
        columns: [
            {"data":"id", "width":"5%"},
            {"data":"name", "width":"25%"},
            {"data":"phoneNumber", "width":"15%"},
            {"data":"applicationUser.email", "width":"15%"},
            {"data":"orderStatus", "width":"15%"},
            {"data":"orderTotal", "width":"10%"},
            {
                "data": "id",
                "render": function (id) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Order/Details?orderId=${id}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i> &nbsp;
                        </a>
                    </div>                    
                    `
                },
                "width": "5%"
            }
        ]
    });
}
