﻿let dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#companiesData').DataTable({
        ajax: {
            url:"/Admin/Company/GetAll"
        },
        columns: [
            {"data":"name", "width":"15%"},
            {"data":"streetAddress", "width":"15%"},
            {"data":"city", "width":"15%"},
            {"data":"state", "width":"15%"},
            {"data":"postalCode", "width":"15%"},
            {"data":"phoneNumber", "width":"15%"},
            {
                "data": "id",
                "render": function (id) {
                    return `
                    <div class="w-75 btn-group" role="group">
                        <a href="/Admin/Company/Upsert?id=${id}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i> &nbsp; Edit
                        </a>
                        <a onclick="deleteByUrl('/Admin/Company/Delete/${id}')" class="btn btn-danger mx-2">
                            <i class="bi bi-trash-fill"></i> &nbsp; Delete
                        </a>
                    </div>                    
                    `
                },
                "width": "15%"
            }
        ]
    });
}

/**
 * call DELETE url with confirmation modal
 *
 * @param {string} url
 */
function deleteByUrl(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function () {
                    dataTable.ajax.reload();
                    toastr.success('Company delete successfully');
                },
                error: function (err) {
                    console.error(err);
                    toastr.error('Cannot delete company');
                },
            })
        }
    })
}