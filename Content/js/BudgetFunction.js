function baseUrl() {

    var href = window.location.href.split("/");

    var url = "";

    url = href[0] + "//" + href[2];
    return url;
}

function openAWindow(pageToLoad, winName, width, height, center) {
    xposition = 0;
    yposition = 0;

    width = $(window).width() - 100;

    height = $(window).height() - 100;

    if ((parseInt(navigator.appVersion) >= 4) && (center)) {
        xposition = (screen.width - width) / 2;
        yposition = (screen.height - height) / 2;
    }

    //0 => no
    //1 => yes
    var args = "";
    args += "width=" +
        width +
        "," +
        "height=" +
        height +
        "," +
        "location=0," +
        "menubar=0," +
        "resizable=0," +
        "scrollbars=1," +
        "statusbar=false,dependent,alwaysraised," +
        "status=false," +
        "titlebar=no," +
        "toolbar=0," +
        "hotkeys=0," +
        "screenx=" +
        xposition +
        "," //NN Only
        +
        "screeny=" +
        yposition +
        "," //NN Only
        +
        "left=" +
        xposition +
        "," //IE Only
        +
        "top=" +
        yposition; //IE Only
    +
        "fullscreen=yes" //IE Only

    var dmcaWin = window.open(baseUrl() + "/" + pageToLoad, winName, args);
    dmcaWin.focus();
    //window.showModalDialog(pageToLoad,"","dialogWidth:650px;dialogHeight:500px");
    return false;

}

function SelectTrasnferMonthFrom(row) {

    var value = $("#Lines_" + row.toString() + "__AccountNameFrom  option:selected").text();

    $("#Lines_" + row.toString() + "__AccountFrom").val(value);

    var TransferType = $("#TransferType option:selected").val();
    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthFrom option:selected").val();



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {

            $("#Lines_" + row.toString() + "__BudgetFromStr").val(data);
            var budgetFrom = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountFromStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetFrom = budgetFrom - Amount;



            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetFrom + '}',
                success: function (data) {

                    $("#txtNewBudgetFrom_" + row.toString()).val(data);

                },
                error: function (data) {

                }
            });




        },
        error: function (data) {

        }
    });

}

function SelectTrasnferMonthTo(row) {

    var value = $("#Lines_" + row.toString() + "__AccountNameTo  option:selected").text();

    $("#Lines_" + row.toString() + "__AccountTo").val(value);

    var TransferType = $("#TransferType option:selected").val();

    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthTo option:selected").val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {

            $("#Lines_" + row.toString() + "__BudgetToStr").val(data);

            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountToStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var budgetTo = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetTo = budgetTo + Amount;



            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetTo + '}',
                success: function (data) {

                    $("#txtNewBudgetTo_" + row.toString()).val(data);

                },
                error: function (data) {

                }
            });


        },
        error: function (data) {

        }
    });

}

function SelectAccountName(row) {
    var value = $("#Lines_" + row.toString() + "__AccountNameFrom  option:selected").text();


    $("#Lines_" + row.toString() + "__AccountFrom").val(value).trigger("chosen:updated");
    var TransferType = $("#TransferType option:selected").val();
    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthFrom option:selected").val();



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {

            $("#Lines_" + row.toString() + "__BudgetFromStr").val(data);




            var budgetFrom = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountFromStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetFrom = budgetFrom - Amount;



            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetFrom + '}',
                success: function (data) {

                    $("#txtNewBudgetFrom_" + row.toString()).val(data);

                },
                error: function (data) {

                }
            });


        },
        error: function (data) {

        }
    });



}

function SelectAccount(row) {
    var value = $("#Lines_" + row.toString() + "__AccountFrom option:selected").text();


    $("#Lines_" + row.toString() + "__AccountNameFrom").val(value).trigger("chosen:updated");
    value = $("#Lines_" + row.toString() + "__AccountFrom option:selected").val();
    var TransferType = $("#TransferType option:selected").val();

    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthFrom option:selected").val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {
            $("#Lines_" + row.toString() + "__BudgetFromStr").val(data);
            var budgetFrom = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountFromStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetFrom = budgetFrom - Amount;



            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetFrom + '}',
                success: function (data) {

                    $("#txtNewBudgetFrom_" + row.toString()).val(data);

                    var Amount = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
                    var budgetFrom = parseFloat($("#Lines_" + row.toString() + "__BudgetFromStr").val().replace(".", "").replace(".", "").replace(",", "."));
                    var newBudgetFrom = budgetFrom - Amount;



                    $.ajax({
                        type: 'Post',
                        contentType: "application/json;",
                        dataType: "json",
                        url: baseUrl() + "/Budget/GetDoubleToStr",
                        data: '{ "value": ' + newBudgetFrom + '}',
                        success: function (data) {

                            $("#txtNewBudgetFrom_" + row.toString()).val(data);

                        },
                        error: function (data) {

                        }
                    });


                },
                error: function (data) {

                }
            });






        },
        error: function (data) {

        }
    });



}

function SelectCostCenterFrom(row) {
    var value = $("#Lines_" + row.toString() + "__BranchFrom option:selected").text();


    $("#Lines_" + row.toString() + "__CostCenterFrom").val(value).trigger("chosen:updated");
}

function SelectBranchFrom(row) {
    var value = $("#Lines_" + row.toString() + "__CostCenterFrom option:selected").text();


    $("#Lines_" + row.toString() + "__BranchFrom").val(value).trigger("chosen:updated");

}

function SelectAccountNameTo(row) {
    var value = $("#Lines_" + row.toString() + "__AccountNameTo  option:selected").text();


    $("#Lines_" + row.toString() + "__AccountTo").val(value).trigger("chosen:updated");
    var TransferType = $("#TransferType option:selected").val();

    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthTo option:selected").val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {

            $("#Lines_" + row.toString() + "__BudgetToStr").val(data);

            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountToStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var budgetTo = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetTo = budgetTo + Amount;

            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetTo + '}',
                success: function (data) {

                    $("#txtNewBudgetTo_" + row.toString()).val(data);

                },
                error: function (data) {

                }
            });


        },
        error: function (data) {

        }
    });



}

function SelectAccountTo(row) {
    var value = $("#Lines_" + row.toString() + "__AccountTo option:selected").text();


    $("#Lines_" + row.toString() + "__AccountNameTo").val(value).trigger("chosen:updated");

    value = $("#Lines_" + row.toString() + "__AccountTo option:selected").val();

    var TransferType = $("#TransferType option:selected").val();

    var TransferMonth = $("#Lines_" + row.toString() + "__TransferMonthTo option:selected").val();

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValue",
        data: '{ "Code": "' + value + '","TransferType": "' + TransferType + '","TransferMonth": "' + TransferMonth + '"}',
        success: function (data) {
            $("#Lines_" + row.toString() + "__BudgetToStr").val(data);


            var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountToStr").val().replace(".", "").replace(".", "").replace(",", "."));
            var budgetTo = parseFloat(data.replace(".", "").replace(".", "").replace(",", "."));
            var newBudgetTo = budgetTo + Amount;

            $.ajax({
                type: 'Post',
                contentType: "application/json;",
                dataType: "json",
                url: baseUrl() + "/Budget/GetDoubleToStr",
                data: '{ "value": ' + newBudgetTo + '}',
                success: function (data) {

                    $("#txtNewBudgetTo_" + row.toString()).val(data);

                },
                error: function (data) {

                }
            });


        },
        error: function (data) {

        }
    });



}

function SelectCostCenterTo(row) {
    var value = $("#Lines_" + row.toString() + "__BranchTo option:selected").text();

    $("#Lines_" + row.toString() + "__CostCenterTo").val(value).trigger("chosen:updated");
}

function SelectBranchTo(row) {
    var value = $("#Lines_" + row.toString() + "__CostCenterTo option:selected").text();


    $("#Lines_" + row.toString() + "__BranchTo").val(value).trigger("chosen:updated");
}

function ChangedAmountTo(row) {

    var budgetTo = parseFloat($("#Lines_" + row.toString() + "__BudgetToStr").val().replace(".", "").replace(".", "").replace(",", "."));

    var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountToStr").val().replace(".", "").replace(".", "").replace(",", "."));

    var newBudgetTo = budgetTo + Amount;

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetDoubleToStr",
        data: '{ "value": ' + newBudgetTo + '}',
        success: function (data) {

            $("#txtNewBudgetTo_" + row.toString()).val(data);

        },
        error: function (data) {

        }
    });


    var budgetFrom = parseFloat($("#Lines_" + row.toString() + "__BudgetFromStr").val().replace(".", "").replace(".", "").replace(",", "."));
    var newBudgetFrom = budgetFrom - Amount;



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetDoubleToStr",
        data: '{ "value": ' + newBudgetFrom + '}',
        success: function (data) {

            $("#txtNewBudgetFrom_" + row.toString()).val(data);

        },
        error: function (data) {

        }
    });
    var value = $("#Lines_" + row.toString() + "__AmountToStr").val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValueStr",
        data: '{ "value": "' + value + '"}',
        success: function (data) {
            $("#Lines_" + row.toString() + "__AmountToStr").val(data);
            $("#Lines_" + row.toString() + "__AmountFromStr").val(data);
        },
        error: function (data) {

        }
    });

}

function ChangedAmountFrom(row) {

    var budgetFrom = parseFloat($("#Lines_" + row.toString() + "__BudgetFromStr").val().replace(".", "").replace(".", "").replace(",", "."));

    var Amount = parseFloat($("#Lines_" + row.toString() + "__AmountFromStr").val().replace(".", "").replace(".", "").replace(",", "."));

    var newBudgetFrom = budgetFrom - Amount;



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetDoubleToStr",
        data: '{ "value": ' + newBudgetFrom + '}',
        success: function (data) {

            $("#txtNewBudgetFrom_" + row.toString()).val(data);

        },
        error: function (data) {

        }
    });

    var budgetTo = parseFloat($("#Lines_" + row.toString() + "__BudgetToStr").val().replace(".", "").replace(".", "").replace(",", "."));



    var newBudgetTo = budgetTo + Amount;

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetDoubleToStr",
        data: '{ "value": ' + newBudgetTo + '}',
        success: function (data) {

            $("#txtNewBudgetTo_" + row.toString()).val(data);

        },
        error: function (data) {

        }
    });

    var value = $("#Lines_" + row.toString() + "__AmountFromStr").val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/GetValueStr",
        data: '{ "value": "' + value + '"}',
        success: function (data) {





            $("#Lines_" + row.toString() + "__AmountFromStr").val(data);
            $("#Lines_" + row.toString() + "__AmountToStr").val(data);
        },
        error: function (data) {

        }
    });

}


function ChangedTransferType() {


    var transferType = $("#TransferType").val();
    if (transferType === "1") {

        var transferMonth = $("#TransferMonth").val();
        for (var i = 0; i < 15; i++) {

            $("#Lines_" + i.toString() + "__TransferMonthFrom").val(transferMonth).trigger("chosen:updated");
            $("#Lines_" + i.toString() + "__TransferMonthTo").val(transferMonth).trigger("chosen:updated");
        }
    } else {


    }

}



function ChangedTransferMonth() {
    var transferType = $("#TransferType").val();
    if (transferType === "1") {

        var transferMonth = $("#TransferMonth").val();
        for (var i = 0; i < 15; i++) {

            $('#txtTransferMonthTo_' + i).val(transferMonth).trigger("chosen:updated");
            $('#txtTransferMonthFrom_' + i).val(transferMonth).trigger("chosen:updated");
        }
    } else {


    }


}

function getProjectListForBudget() {
    var BegDate = $('#begdate').val();
    var EndDate = $('#enddate').val();
    var supplier = $('#supplier').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/setProjectListFilter",
        data: '{ "BegDate": "' + BegDate + '","EndDate": "' + EndDate + '" ,"supplier": "' + supplier + '"}',
        success: function (data) {
            window.location.href = window.location.href;


        },
        error: function (data) {

        }
    });
}


function setTransferValues(lineNr) {

    var month = $("#Lines_" + lineNr + "__TransferMonthFrom option:selected").val();
    var account = $("#Lines_" + lineNr + "__AccountNameFrom option:selected").val();
    var CostCenter = $("#Lines_" + lineNr + "__BranchFrom option:selected").val();
    var department = $("#Lines_" + lineNr + "__DepartmentFrom option:selected").val();
    var amount = $('#Lines_' + lineNr + '__AmountFromStr').val();


    $('#txtTransferMonthFrom_' + lineNr).val(month);
    $('#txtTransferAccountFrom_' + lineNr).val(account);
    $('#txtTransferCostCenterFrom_' + lineNr).val(CostCenter);
    $('#txtTransferDepatmentFrom_' + lineNr).val(department);
    $('#txtTransferAmountFrom_' + lineNr).val(amount);



    month = $("#Lines_" + lineNr + "__TransferMonthTo option:selected").val();
    account = $("#Lines_" + lineNr + "__AccountNameTo option:selected").val();
    CostCenter = $("#Lines_" + lineNr + "__BranchTo option:selected").val();
    department = $("#Lines_" + lineNr + "__DepartmentTo option:selected").val();
    amount = $('#Lines_' + lineNr + '__AmountToStr').val();


    $('#txtTransferMonthTo_' + lineNr).val(month);
    $('#txtTransferAccountTo_' + lineNr).val(account);
    $('#txtTransferCostCenterTo_' + lineNr).val(CostCenter);
    $('#txtTransferDepatmentTo_' + lineNr).val(department);
    $('#txtTransferAmountTo_' + lineNr).val(amount);





    $('#ModalBudgetTransferLine_' + lineNr).modal('hide');
}

function getBudgetDetail(budgetLineId) {


    $('#txtBudgetLineId').val(budgetLineId);
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/getBudgetDetail",
        data: '{ "budgetLineId": ' + budgetLineId + '}',
        success: function (data) {

            $('#txtTransferMonthFrom').val(data.TransferMonthFrom);
            $('#txtDepartmentFrom').val(data.DepartmentFrom);
            $('#txtBranchFrom').val(data.CostCenterFrom);
            $('#txtAccountNameFrom').val(data.AccountFrom);
            $('#txtCostCenterFrom').val(data.BranchFrom);
            $('#txtAccountFrom').val(data.AccountNameFrom);
            $('#txtAmountFromStr').val(data.AmountFromStr);
            $('#txtBudgetFromStr').val(data.BudgetFromStr);
            $('#txtNewBudgetFrom').val(data.NewBudgetFromStr);

            $('#txtTransferMonthTo').val(data.TransferMonthTo);
            $('#txtDepartmentTo').val(data.DepartmentTo);
            $('#txtBranchTo').val(data.CostCenterTo);
            $('#txtAccountNameTo').val(data.AccountTo);
            $('#txtCostCenterTo').val(data.BranchTo);
            $('#txtAccountTo').val(data.AccountNameTo);
            $('#txtAmountToStr').val(data.AmountToStr);
            $('#txtBudgetToStr').val(data.BudgetToStr);
            $('#txtNewBudgetTo').val(data.NewBudgetToStr);

            $('#txtDemandPerson').val(data.DemandPerson);

            $('#BudgetExp').val(data.BudgetExp);
            $('#ModalBudgetTransferLine').modal('show');

            if (data.BudgetExpList.length > 1) {

                html = '';
                for (var i = 1; i < data.BudgetExpList.length; i++) {
                    if (data.BudgetExpList[i] != "") {

                  ///      html = html + '<div class=\"col-lg-12\">';
                        html = html + '<div class=\"form-group-inner \">';
                        html = html + '<textarea class=\"form-control textinput\" value="" cols=\"20\"  name=\"txtRejectResponse\" rows=\"5\" style=\"background-color:transparent\">';
                        html = html + ' ' + data.BudgetExpList[i] + '';
                        html = html + '</textarea>';
                        html = html + '<hr style=\"margin:3px\" />';
                        html = html + '</div>';
                 //       html = html + '</div>';
                    }
                }
                $('#divDetailExpList').html(html);
                
                $('#ModalDetailExpList').modal('show');

            }
        },
        error: function (data) {

        }
    });

}
function SetRejectBudget() {

    $('#rejectBudgetArea').removeClass("hidden");
    $('#RejectfooterArea').removeClass("hidden");
    $('#confirmBudgetArea').addClass("hidden");
    $('#BudgetArea').addClass("hidden");
    $('#footerArea').addClass("hidden");

}
function SetConfirmBudget() {
    $('#confirmBudgetArea').removeClass("hidden");
    $('#confirmfooterArea').removeClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#BudgetArea').addClass("hidden");
    $('#footerArea').addClass("hidden");
}
function exitRejectBudget() {
    $('#BudgetArea').removeClass("hidden");
    $('#confirmBudgetArea').addClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#footerArea').removeClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#RejectfooterArea').addClass("hidden");

}
function exitConfirmBudget() {
    $('#BudgetArea').removeClass("hidden");
    $('#confirmBudgetArea').addClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#footerArea').removeClass("hidden");
    $('#confirmfooterArea').addClass("hidden");
    $('#RejectfooterArea').addClass("hidden");

}
function RejectBudget() {
    var BudgetLineId = $('#txtBudgetLineId').val();
    alert(BudgetLineId);
    $('#BudgetArea').removeClass("hidden");
    $('#confirmBudgetArea').addClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#footerArea').removeClass("hidden");
    $('#confirmfooterArea').addClass("hidden");
    $('#RejectfooterArea').addClass("hidden");
    $('#ModalBudgetTransferLine').modal('hide');


    var BudgetConfirmExp = $('#txtBudgetRejectExp').val();

    if (BudgetConfirmExp != "") {


        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Budget/updateBudgetTransferConfirm",
            data: '{ "budgetLineId": ' + BudgetLineId + ',"status": 2 ,"exp": "' + BudgetConfirmExp + '"}',
            success: function (data) {



            },
            error: function (data) {

            }
        });
    } else {

        alert("Açýklama Giriniz..");
    }


}
function ConfirmBudget() {
    var BudgetLineId = $('#txtBudgetLineId').val();
    alert(BudgetLineId);

    $('#BudgetArea').removeClass("hidden");
    $('#confirmBudgetArea').addClass("hidden");
    $('#rejectBudgetArea').addClass("hidden");
    $('#footerArea').removeClass("hidden");
    $('#confirmfooterArea').addClass("hidden");
    $('#RejectfooterArea').addClass("hidden");
    $('#ModalBudgetTransferLine').modal('hide');

    var BudgetConfirmExp = $('#txtBudgetConfirmExp').val();



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/updateBudgetTransferConfirm",
        data: '{ "budgetLineId": ' + BudgetLineId + ',"status": 1 ,"exp": "' + BudgetConfirmExp + '"}',
        success: function (data) {



        },
        error: function (data) {

        }
    });

}
function getRejectDetail(BudgetLineId) {


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Budget/getRejectDetail",
        data: '{ "budgetLineId": ' + BudgetLineId + '}',
        success: function (data) {


            $('#txtRejectedDetailId').val(BudgetLineId);
            $('#txtRejectedPerson').val(data.PersonName);
            $('#txtRejectExp').val(data.Comment);
            $('#ModalRejectDetail').modal('show');
        },
        error: function (data) {

        }
    });

}

function sendToConfirm() {
    var BudgetLineId = $('#txtRejectedDetailId').val();
    var RejectResponse = $('#txtRejectResponse').val();

    if (RejectResponse === "") {

        alert("Aciklama Giriniz..");

    }
    else {
        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Budget/sendToConfirm",
            data: '{ "budgetLineId": ' + BudgetLineId + ' ,"RejectResponse": "' + RejectResponse + '"}',
            success: function (data) {
                if (data) {
                    window.location.href = window.location.href;
                }

            },
            error: function (data) {

            }
        });
    }
}
