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


function closeModal(modalName) {

    $('#' + modalName).modal('hide');
}

function createOffer() {

    var rowCount = $("#DemandTable >tbody >tr").length;
    var transrefList = [];
    for (var i = 0; i < rowCount; i++) {
        var color = $('#cboxDemand_' + i.toString()).css("background-color").toString();
        if (color == 'rgb(30, 144, 255)') {
            var transRef = $('#Demands_' + i.toString() + "__TransRef").val();
            transrefList.push(transRef);
        }
    }
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/CreateOffer",
        data: JSON.stringify(transrefList),
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });


}

function SaveDocumentName(lineNr) {
    var docName = $("#DocList_" + lineNr + "__DocumentName").val();
    var docId = $("#DocList_" + lineNr + "__Id").val();
    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/SaveDocumentName",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"docName": "' + docName + '" ,"docId": "' + docId + '" ,"ProjectId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerDocumentName(data);

            $("#OfferDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function SaveOrderDocumentName(lineNr) {
    var docName = $("#DocList_" + lineNr + "__DocumentName").val();
    var docId = $("#DocList_" + lineNr + "__Id").val();
    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/SaveOrderDocumentName",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"docName": "' + docName + '" ,"docId": "' + docId + '" ,"ProjectId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerDocumentName(data);

            $("#OrderDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function DeleteDocument(lineNr) {

    var docId = $("#DocList_" + lineNr + "__Id").val();
    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/DeleteDocument",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"docId": "' + docId + '" ,"ProjectId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerDocumentName(data);

            $("#OfferDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function DeleteOrderDocument(lineNr) {

    var docId = $("#DocList_" + lineNr + "__Id").val();
    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/DeleteOrderDocument",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"docId": "' + docId + '" ,"OrderId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerOrderDocumentName(data);

            $("#OrderDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function getDocument() {


    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/GetDocument",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"ProjectId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerDocumentName(data);

            $("#OfferDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function getOrderDocument() {


    var ProjectId = $("#ProjectId").val();
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getOrderDocument",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"ProjectId": "' + ProjectId + '"}',
        success: function(data) {

            var html = successHandlerOrderDocumentName(data);

            $("#OrderDocumentTableBody").html(html);
        },
        error: function(data) {


        }
    });
}

function successHandlerDocumentName(data) {
    var rowCount = data.length;


    var html = "";

    for (var i = 0; i < rowCount; i++) {

        html = html + "<tr>";

        html = html + "<td width=\"35%\">" + data[i].UploadedFileName + "</td>";
        html = html + "<td width=\"30%\">";
        html = html + "<input class=\"form-control hidden\" id=\"DocList_" + i + "__Id\" name=\"DocList[" + i + "].Id\" type=\"text\" value=\"" + data[i].Id + "\" />";
        html = html + "<input class=\"form-control\" data-val=\"true\"  id=\"DocList_" + i + "__DocumentName\" name=\"DocList[" + i + "].DocumentName\" style=\"font-weight: bold;font-size:12px !Important;width:100% !important;\" type=\"text\" value=\"" + data[i].DocumentName + "\" />";
        html = html + "</td>";
        html = html + "<td width=\"18%\">";
        html = html + "<a role=\"button\" onclick=\"openAWindow('Demands/FileView?DocumentId=" + data[i].Id + "', 'Doküman Ýzleme' , 920, 640, true)\" class=\"btn btn-info\"><i class=\"fa fa-street-view\"> </i> Incele</a>";
        html = html + "</td>";
        html = html + "<td width=\"17%\">";
        html = html + "<a role=\"button\" onclick=\"SaveDocumentName('" + i + "')\"  class=\"btn btn-success\"><i class=\"fa fa-save\"> </i> Kaydet</a>";
        html = html + "</td>";
        html = html + "<td width=\"10%\">";
        html = html + "<a role=\"button\" onclick=\"DeleteDocument('" + i + "')\"  class=\"btn btn-danger\"><i class=\"fa fa-remove\"></i> Sil</a>";
        html = html + "</td>";
        html = html + "</tr>";
    }

    return html;
}

function successHandlerOrderDocumentName(data) {
    var rowCount = data.length;


    var html = "";

    for (var i = 0; i < rowCount; i++) {

        html = html + "<tr>";

        html = html + "<td width=\"35%\">" + data[i].UploadedFileName + "</td>";
        html = html + "<td width=\"30%\">";
        html = html + "<input class=\"form-control hidden\" id=\"DocList_" + i + "__Id\" name=\"DocList[" + i + "].Id\" type=\"text\" value=\"" + data[i].Id + "\" />";
        html = html + "<input class=\"form-control\" data-val=\"true\"  id=\"DocList_" + i + "__DocumentName\" name=\"DocList[" + i + "].DocumentName\" style=\"font-weight: bold;font-size:12px !Important;width:100% !important;\" type=\"text\" value=\"" + data[i].DocumentName + "\" />";
        html = html + "</td>";
        html = html + "<td width=\"18%\">";
        html = html + "<a role=\"button\" onclick=\"openAWindow('Order/FileView?DocumentId=" + data[i].Id + "', 'Doküman Ýzleme' , 920, 640, true)\" style=\"height:20px; padding:2px\" class=\"btn btn-info\"><i class=\"fa fa-street-view\"> </i> Incele</a>";
        html = html + "</td>";
        html = html + "<td width=\"17%\">";
        html = html + "<a role=\"button\" onclick=\"SaveOrderDocumentName('" + i + "')\" style=\"height:20px; padding:2px\" class=\"btn btn-success\"><i class=\"fa fa-save\"> </i> Kaydet</a>";
        html = html + "</td>";
        html = html + "<td width=\"10%\">";
        html = html + "<a role=\"button\" onclick=\"DeleteOrderDocument('" + i + "')\" style=\"height:20px; padding:2px\" class=\"btn btn-danger\"><i class=\"fa fa-remove\"></i> Sil</a>";
        html = html + "</td>";
        html = html + "</tr>";
    }

    return html;
}

function saveSerie(lineNr) {
    var orderAmount = parseFloat($("#series_" + lineNr + "__OrderAmount").val().replace(",", "."));

    var Net_Series = {
        Id: $("#series_" + lineNr + "__Id").val(),
        SeriName: $("#tdId_" + lineNr).text(),
        ArriveTime: $("#series_" + lineNr + "__ArriveTime").val(),
        OrderAmount: orderAmount,
        MinStockMonth: $("#series_" + lineNr + "__MinStockMonth").val()
    };

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Home/saveSerie",
        data: Net_Series,
        success: function(data) {

            Lobibox.notify(data.ResType, {
                showClass: "fadeInDown",
                hideClass: "fadeUpDown",
                title: data.Title,
                msg: data.Message
            });

        },

    });
    return false;
}




function humanizeNumber(price) {
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Project/getPriceStr",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: '{"price": "' + price + '"}',
        success: function(data) {
            $("#TotalOrderPrice").val(data);
        },
        error: function(data) {
            $("#TotalOrderPrice").val("00.0000 USD");

        }
    });
}


function humanizePrice(price) {
    var result = "";
    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getPriceStr",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        data: '{"price": "' + price + '"}',
        success: function(data) {
            result = data;
        },
        error: function(data) {


        }
    });
    return result;
}


function getSupplier() {

    var SupplierCode = $("#search_SupplierCode").val();
    var SupplierDesc = $("#search_SupplierDesc").val();


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/GetSuppliers",
        data: '{ "SupplierCode": "' + SupplierCode + '","SupplierDesc": "' + SupplierDesc + '"}',
        success: function(data) {
            var html = successHandlerSupplier(data);

            $("[id*='supplierList']").html(html);

        },
        error: function(data) {

        }
    });


}

function successHandlerSupplier(data) {

    var rowCount = data.length;
    //SupplierRef
    //SupplierCode
    //SupplierDesc

    var html = "";

    for (var i = 0; i < rowCount; i++) {




        html = html + "<tr> ";
        html = html + "     <td> " + (i + 1).toString() + "</td> ";
        html = html + "    <td>" + data[i].SupplierCode + "</td> ";
        html = html + "    <td>" + data[i].SupplierDesc + "</td> ";
        html = html + "    <td align=\"right\"> ";
        html = html + "        <div class=\"form-group-inner\"> ";
        html = html + "            <div class=\"bt-df-checkbox\"> ";
        html = html + "                <div class=\"i-checks\"> ";
        html = html + "                    <label> ";
        html = html + "    <script type=\"text/javascript\"> ";
        html = html + "    $(\'#cbox_" + i + "').change(function () {  ";
        html = html + "  checkSupplier(" + data[i].SupplierRef + ", " + i + ")   ";
        html = html + "  }); ";
        html = html + "  </script > ";
        if (data[i].selectSupplier) {
            html = html + " <input style=\"font-size: 18px\" id=\"cbox_" + i + "\" name=\"cboxSelectSupplier\" type=\"checkbox\" value=\"\" checked=\"checked\" > <i></i>  ";
        } else {
            html = html + " <input style=\"font-size: 18px\" id=\"cbox_" + i + "\" name=\"cboxSelectSupplier\" type=\"checkbox\" value=\"\" > <i></i>  ";
        }

        html = html + "                    </label> ";
        html = html + "                </div> ";
        html = html + "            </div> ";
        html = html + "         </div> ";
        html = html + "    </td> ";
        html = html + "</tr> ";

    }

    return html;
}

function checkSupplier(SupplierRef, row) {

    var add = 0;
    if ($("#cbox_" + row.toString()).is(":checked")) {
        add = 1;
    }
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/SetSupplierSession",
        data: '{ "SupplierRef": ' + SupplierRef + ' , "add": ' + add + '}',
        success: function(data) {


        },
        error: function(data) {

        }
    });


}

function SelectSupplier() {
    var projectId = $("#ProjectId").val();

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/SelectSupplier",
        data: '{ "projectId": ' + projectId + ' }',
        success: function(data) {
            var html = successHandlerSelectSupplier(data);
            $("[id*='OfferSupplierList']").append(html);
            if (data.length > 0) {
                $('#supplierAddedWarning').removeClass("hidden");
            }
        },
        error: function(data) {

        }
    });


}

function setPriceWithTax(rowNr) {


    var usdCurr = $('#txtUsdCurr').val();
    var eurCurr = $('#txtEurCurr').val();

    var dovizCurr = 1.0;

    var vatrate = $('#Lines_' + rowNr + '__VatRate').val();


    var newPrice = $('#Lines_' + rowNr + '__NewPrice').val();
    newPrice = newPrice.replace('.', '');

    var Quantity = $('#Lines_' + rowNr + '__Quantity').val();

    var TrCurr = $('#Lines_' + rowNr + '__TrCurr').val();


    var dbl_vatrate = parseFloat(vatrate.replace('.', '').replace('.', '').replace(',', '.'));
    var dbl_newPrice = parseFloat(newPrice.replace('.', '').replace('.', '').replace(',', '.'));


    var dbl_Quantity = parseFloat(Quantity.replace('.', '').replace('.', '').replace(',', '.'));
    if (TrCurr == "1") {
        dovizCurr = parseFloat(usdCurr.replace(',', '.'));
    }
    if (TrCurr == "20") {
        dovizCurr = parseFloat(eurCurr.replace(',', '.'));
    }

    var dbl_priceWithTax = dbl_newPrice + ((dbl_newPrice / 100) * dbl_vatrate);
    var strpriceWithTax = humanizePrice(dbl_priceWithTax);

    $('#Lines_' + rowNr + '__NewPriceWithVat').val(strpriceWithTax);

    var dbl_TotalWithTax = dbl_priceWithTax * dovizCurr * dbl_Quantity;
    var str_TotalWithTax = humanizePrice(dbl_TotalWithTax);
    $('#Lines_' + rowNr + '__NetTotalWithTax').val(str_TotalWithTax);


    var rowCount = $("#OfferLineTable >tbody >tr").length;
    var subTotal = 0.0;
    for (var i = 0; i < rowCount; i++) {
        var lineTotal = $('#Lines_' + i + '__NetTotalWithTax').val();
        lineTotal = lineTotal.replace('.', '').replace('.', '');
        var dbl_lineTotal = parseFloat(lineTotal.replace(',', '.'));
        subTotal = subTotal + dbl_lineTotal;

    }


    var subTotal_str = humanizePrice(subTotal);

    $('#txtSubTotal').val(subTotal_str);

    //  var str_newPrice = humanizePrice(dbl_newPrice);
    $('#Lines_' + rowNr + '__NewPrice').val(str_newPrice);


}


function successHandlerSelectSupplier(data) {

    var ColumnNumber = $("#SupplierTable > thead > tr:first > th").length - 3;
    var rowNumber = $("#SupplierTable >tbody >tr").length;
    var rowCount = data.length;
    var control = false;
    var html = "";
    var controlHtml = " ";
    for (var i = 0; i < rowCount; i++) {
        if (data[i] != null) {

            if (!data[i].isControlled) {
                html = html + "<tr> ";
                html = html + "     <td  align=\"center\"></td> ";
                html = html + "     <td  align=\"center\"></td> ";
                html = html + "     <td align=\"center\">" + ((rowNumber) + i + 1).toString() + "</td> ";
                html = html + "    <td> ";
                html = html + "     <input class=\"form-control hidden\"  id=\"Suppliers_" + i + "__SupplierRef\" name=\"Suppliers[" + i + "].SupplierRef\" type=\"text\" value=\"" + data[i].SupplierRef.toString() + "\" />  ";

                html = html + "        <input class=\"form-control\" id=\"Suppliers_" + i + "__SupplierCode\" name=\"Suppliers[" + i + "].SupplierCode\" readonly=\"readonly\" style=\"font-weight:bold; font-size:12px!Important; width:100% !important\" type=\"text\" value=\"" + data[i].SupplierCode + "\" />  ";
                html = html + "    </td> ";
                html = html + "     <td> ";
                html = html + "         <input class=\"form-control\" id=\"Suppliers_" + i + "__SupplierDesc\" name=\"Suppliers[" + i + "].SupplierDesc\" readonly=\"readonly\" style=\"font-weight:bold; font-size: 12px!Important; width:100% !important\" type=\"text\" value=\"" + data[i].SupplierDesc + "\" />  ";
                html = html + "    </td>  ";

                for (var j = 0; j < ColumnNumber; j++) {

                    html = html + "     <td align=\"center\"></td> ";
                }
                html = html + " </tr> ";
            } else {

                controlHtml = controlHtml + data[i].SupplierCode + " - " + data[i].SupplierDesc + "</br>";
                control = true;


            }
        }
    }


    if (control) {
        $("[id*='supplierControl']").html(controlHtml);

        $('#ModalSupplierControl').modal('show');
    }



    return html;
}

function GetDemands() {
    var Ntl_DemandFilter = {
        SlipNr: $("#search_SlipNr").val(),
        SlipBegDate: $("#search_SlipBegDate").val(),
        SlipEndDate: $("#search_SlipEndDate").val(),
        ItemGrpCode: $("#search_ItemGrpCode").val(),
        ItemCode: $("#search_ItemCode").val(),
        ItemDesc: $("#search_ItemDesc").val(),
        Person: $("#search_Person").val(),
        Department: $("#search_Department").val(),
        Usage: $("#search_Usage").val()
    };



    $.ajax({
        type: 'Post',
        url: baseUrl() + "/Ajax/GetDemands",
        data: Ntl_DemandFilter,
        success: function(data) {
            var html = successHandlerDemands(data);
            $("[id*='demandsList']").html(html);
        },
        error: function(data) {

        }
    });


}

function GetRequirementDemands() {
    var Ntl_DemandFilter = {
        SlipNr: $("#search_SlipNr").val(),
        SlipBegDate: $("#search_SlipBegDate").val(),
        SlipEndDate: $("#search_SlipEndDate").val(),
        ItemGrpCode: $("#search_ItemGrpCode").val(),
        ItemCode: $("#search_ItemCode").val(),
        ItemDesc: $("#search_ItemDesc").val(),
        Person: $("#search_Person").val(),
        Deparment: $("#search_Deparment").val(),
        Usage: $("#search_Usage").val()
    };



    $.ajax({
        type: 'Post',
        url: baseUrl() + "/Ajax/GetRequirementDemands",
        data: Ntl_DemandFilter,
        success: function(data) {
            var html = successHandlerDemands(data);
            $("[id*='demandsList']").html(html);
        },
        error: function(data) {

        }
    });


}

function successHandlerDemands(data) {

    var rowCount = data.length;
    var html = "";
    for (var i = 0; i < rowCount; i++) {

        html = html + " <tr style=\"font-size:14px\"> ";


        html = html + "         <td>";
        html = html + "        <input class=\"form-control hidden\"id=\"Demands_" + i.toString() + "__TransRef\" name=\"Demands[" + i.toString() + "].TransRef\" type=\"text\" value=\"" + data[i].TransRef + "\" /> ";
        html = html + " <a onclick=\"DemandChecked(" + i + ", " + data[i].TransRef + ")\" name=\"cboxDemand\" id=\"cboxDemand_" + i.toString() + "\" style=\"margin-top:0px;background-color:transparent;width:34px; border:1px solid dodgerblue\" class=\"btn btn-rounded  btn-success\"><i class=\"fa fa-check \" aria-hidden=\"true\"></i> </a>";

        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].SlipNr;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].SlipDateStr;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].ItemGrpCode;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].ItemCode;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].ItemDesc;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].Quantity;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].Unit;
        html = html + "         </td> ";

        var myObj = {
            style: "decimal",
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }
        var lineNet = data[i].LineNet.toLocaleString("tr-TR", myObj)
        html = html + "         <td style=\"text-align:right\"> ";
        html = html + lineNet;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].PersonName;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].Department;

        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + "             <button class=\"btn btn-warning\" style=\"width:100%; font-size:14px; height:20px; margin:2px ; padding:1px\">0,00</button> ";
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].LineExp;
        html = html + "         </td> ";
        html = html + "         <td> ";
        html = html + data[i].Usage;
        html = html + "         </td> ";
        html = html + "     </tr> ";
    }

    return html;
}

function budgetControl(ProjectId, SupplierRef, row) {


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/BudgetControl",
        data: '{ "ProjectId": "' + ProjectId + '" , "SupplierRef": "' + SupplierRef + '"}',
        success: function(data) {

            var rowCount = data.length;
            var html = "";
            var budgetOk = 0;
            for (var i = 0; i < rowCount; i++) {
                html = html + " <tr style=\"font-size:14px\"> ";
                html = html + "         <td> ";
                html = html + data[i].BudgetInfo;
                html = html + "         </td> ";
                html = html + "     </tr> ";
                if (data[i].BudgetOk == false) {
                    budgetOk = budgetOk + 1;
                }

            }


            if (budgetOk == 0) {
                $('#btnSendOrder_' + row).removeAttr("disabled");
                $('#btnSendContract_' + row).removeAttr("disabled");
            }

            $("[id*='BudgetControlTable']").html(html);
            $('#ModalBudgetControl').modal('show');
        },
        error: function(data) {

        }
    });


}

function sendAsOrder(ProjectId, SupplierRef) {

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/sendAsOrder",
        data: '{ "ProjectId": "' + ProjectId + '","SupplierRef": "' + SupplierRef + '"}',
        success: function(data) {
            location.reload();

        },
        error: function(data) {

        }
    });


}

function saveComment(commentId) {
    var comment = $('#txtYourComment').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Comment/saveComment",
        data: '{ "comment": "' + comment + '","commentId": "' + commentId + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });

    window.close();
}

function saveProjectComment(projectId) {
    var comment = $('#txtYourComment').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Demands/saveProjectComment",
        data: '{ "comment": "' + comment + '","projectId": "' + projectId + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });

    window.close();
}

function saveProjectExplanation(projectId) {
    var explanation = $('#txtProjectExplanation').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/saveProjectExplanation",
        data: '{ "projectId": "' + projectId + '","explanation": "' + explanation + '"}',
        success: function(data) {
            Lobibox.notify("success", {
                showClass: "fadeInDown",
                hideClass: "fadeUpDown",
                title: "Basarili",
                msg: "Aciklama Kayit edildi."
            });
        },
        error: function(data) {

        }
    });


}

function saveProjectOfferExplanation(projectId) {
    var explanation = $('#Explanation').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/saveProjectOfferExplanation",
        data: '{ "projectId": "' + projectId + '","explanation": "' + explanation + '"}',
        success: function(data) {
            Lobibox.notify("success", {
                showClass: "fadeInDown",
                hideClass: "fadeUpDown",
                title: "Basarili",
                msg: "Aciklama Kayit edildi."
            });
        },
        error: function(data) {

        }
    });


}

function sendAsContract(ProjectId, SupplierRef) {

    window.close();

}

function SupplierChecked(Id, ProjectId) {

    var color = $('#cboxSupplier_' + Id).css("background-color").toString();

    $("a[name='cboxSupplier']").css('background-color', 'white');


    var person = $('#drpCommentPerson').val();

    if (color == 'rgb(30, 144, 255)') {

        $('#cboxSupplier_' + Id).css('background-color', 'white');
        Id = 0;
        $('#btnGetConfirm').addClass("disabled");
        $('#btnGetComment').addClass("disabled");




    } else {

        $('#cboxSupplier_' + Id).css('background-color', 'dodgerblue');
        $('#btnGetConfirm').removeClass("disabled");
        $('#btnGetComment').removeClass("disabled");

    }

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/updateProjectSupplier",
        data: '{ "SuggestionSupplierRef": ' + Id + ',"ProjectId": ' + ProjectId + '}',
        success: function(data) {

        },
        error: function(data) {

        }
    });



    getProjectStatus();
}


function demandCheckedAll(Id, ProjectId) {
    var color = $('#cboxDemand_All').css("background-color").toString();

    if (color == 'rgb(30, 144, 255)') {
        $('#cboxDemand_All').css('background-color', 'white');
        $("a[name='cboxDemand']").css('background-color', 'white');

    } else {
        $('#cboxDemand_All').css('background-color', 'dodgerblue');
        $("a[name='cboxDemand']").css('background-color', 'dodgerblue');
    }

}

function DemandChecked(row, transref) {

    var color = $('#cboxDemand_' + row).css("background-color").toString();


    if (color == 'rgb(30, 144, 255)') {

        $('#cboxDemand_' + row).css('background-color', 'white');
    } else {
        $('#cboxDemand_' + row).css('background-color', 'dodgerblue');

    }




}

function saveUnConfirm(confirmId) {
    var comment = $('#txtYourComment').val();
    if (comment == "") {

        Lobibox.notify("error", {
            showClass: "fadeInDown",
            hideClass: "fadeUpDown",
            title: "Basarisiz",
            msg: "Yorum yazmaniz gerekiyor."
        });

    } else {

        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Confirm/saveConfirm",
            data: '{ "comment": "' + comment + '","confirmId": "' + confirmId + '","confirmStatus" :2}',
            success: function(data) {
                window.location.href = window.location.href;
            },
            error: function(data) {

            }
        });
    }
}

function saveConfirm(confirmId) {
    var comment = $('#txtYourComment').val();


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Confirm/saveConfirm",
        data: '{ "comment": "' + comment + '","confirmId": "' + confirmId + '","confirmStatus" :1}',
        success: function(data) {
            window.location.href = window.location.href;
        },
        error: function(data) {
            window.location.href = window.location.href;
        }
    });


    window.location.href = window.location.href;
}


function getCurrency() {
    var uuu = baseUrl();

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getLastestCurr",
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function(data) {
            $('#txtUsdCurr').val(data.Usd);
            $('#txtEurCurr').val(data.Eur);
        },

    });
    return false;
}

function createDemand() {

    var rowCount = $("#DemandTable >tbody >tr").length;
    var transrefList = [];
    for (var i = 0; i < rowCount; i++) {
        var color = $('#cboxDemand_' + i.toString()).css("background-color").toString();
        if (color == 'rgb(30, 144, 255)') {
            var transRef = $('#Demands_' + i.toString() + "__TransRef").val();
            transrefList.push(transRef);
        }
    }
    if (transrefList.length > 0) {

        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Ajax/CreateDemand",
            data: JSON.stringify(transrefList),
            success: function(data) {
                window.location.href = data;
            },
            error: function(data) {

            }
        });

    } else {
        Lobibox.notify("error", {
            showClass: "fadeInDown",
            hideClass: "fadeUpDown",
            title: "Uyari",
            msg: "Talep Seciniz."
        });
    }
}

function sendReminder(requestId) {



    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/sendReminder",
        data: '{ "requestId": "' + requestId + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });



}

function sendInvConfirm(projectId) {
    var Explanation = $('#item_Explanation').val();


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/SendInvoiceConfirm",
        data: '{ "ProjectId": ' + projectId + ' , "Explanation": "' + Explanation + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });



}

function confirmInvoice(ConfirmGuid) {
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/confirmInvoice",
        data: '{ "ConfirmGuid": "' + ConfirmGuid + '" }',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });



}


function setLowestTotal() {
    var rowCount = $("#SupplierTable >tbody >tr").length;
    var colCount = parseInt($('#OfferCount').val());


    var lowestTotalRow = 0;
    var lowestTotalCol = 0;
    var lowestTotal = 0.0;
    for (var i = 0; i < rowCount; i++) {

        for (var j = 0; j < colCount; j++) {
            var nettotal = "0.00";
            if ($('#Suppliers_' + i + '__NetTotals_' + j + '__NetTotal').length) {

                nettotal = $('#Suppliers_' + i + '__NetTotals_' + j + '__NetTotal').val();
            }
            nettotal = nettotal.replace('.', '');
            var dbl_nettotal = parseFloat(nettotal.replace(',', '.'));

            var color = $('#Suppliers_' + i + '__NetTotals_' + j + '__NetTotal').css("background-color").toString();

            if (dbl_nettotal > 0 && color != 'rgb(255, 0, 0)') {
                $('#Suppliers_' + i + '__NetTotals_' + j + '__NetTotal').css('background-color', '#eee');
                if (dbl_nettotal < lowestTotal || lowestTotal == 0) {

                    lowestTotal = dbl_nettotal;
                    lowestTotalCol = j;
                    lowestTotalRow = i;
                }
            }


        }



    }
    $('#Suppliers_' + lowestTotalRow + '__NetTotals_' + lowestTotalCol + '__NetTotal').css('background-color', '#5cb85c');
}

function ChangedCommentDepartment() {

    $('#drpCommentPerson')[0].options.length = 1;

    var department = $('#drpCommentDepartment').val();
    if (department == "") {
        $('#bntGetCommentModal').addClass("disabled");

    }

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getDepartmentPerson",
        contentType: 'application/json; charset=utf-8',
        data: '{ "department": "' + department + '"}',
        dataType: 'json',
        success: function(data) {
            for (var i = 0; i < data.length; i++) {

                $("#drpCommentPerson").append('<option value="' + data[i].value + '">' + data[i].name + '</option>');

            }


        },

    });

}

function ChangedCommentPerson() {
    var person = $('#drpCommentPerson').val();
    var personName = $("#drpCommentPerson option:selected").html();
    var ProjectId = $('#ProjectId').val();
    if (person == "") {
        $('#bntGetCommentModal').addClass("disabled");


    } else {
        $('#bntGetCommentModal').removeClass("disabled");

        $.ajax({
            type: "POST",
            url: baseUrl() + "/Ajax/UpdateCommentPerson",
            contentType: 'application/json; charset=utf-8',
            data: '{ "mail": "' + person + '" ,"person": "' + personName + '" , "projectId": ' + ProjectId + '  }',
            dataType: 'json',
            success: function(data) {



            },

        });

    }


}

function ChangedStatusWaitingOrder() {
    var status = $('#drpStatusList').val();
    var statusExp = "";

    if (status === "0") {
        statusExp = "0";
    } else if (status === "1") {
        statusExp = "Bekliyor";
    } else if (status === "2") {
        statusExp = "Onay Bekleniyor";
    } else if (status === "3") {
        statusExp = "Onaylanmadan geri geldi";
    } else if (status === "4") {
        statusExp = "Onaylandý";
    }


    var PrjNo = $('#txtSearch_PrjNo').val();


    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getProjectWaitinOrder",
        data: '{ "PrjNo": "' + PrjNo + '", "ProjectStatus": "' + status + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });



}


function getProjectStatus() {
    var ProjectId = $('#ProjectId').val();

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getProjectStatus",
        contentType: 'application/json; charset=utf-8',
        data: '{ "ProjectId": ' + ProjectId + '  }',
        dataType: 'json',
        success: function(data) {
            $('#lblProjectStatus').text(data.ProjectStatus);
            if (data.SuggestionSupplierRef === 0) {

            } else if (data.PaymentPlan == 0) {


            } else {

                if (data.ConfirmPerson != "") {
                    $('#btnWaitingConfirm').removeClass("hidden");
                    $("a").addClass("disabled");
                    $("textarea").attr("disabled", "disabled");
                } else if (data.RejectPerson != "") {
                    $('#btnGetConfirm').removeClass("hidden");
                } else {
                    if (data.ConfirmedPersonel > 0) {
                        $('#btnOrderSend').removeClass("hidden");
                        $('#btnConfirmed').removeClass("hidden");
                    } else {
                        $('#btnGetComment').removeClass("hidden");
                        $('#btnGetConfirm').removeClass("hidden");
                    }
                }
            }
            if (data.CommentCount == 0) {
                $("#btnComment").addClass("disabled");

            } else {
                $("#btnComment").removeClass("disabled");
            }

        },
    });


}

function getConfirmProjectStatus() {
    var ProjectId = $('#ProjectId').val();

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getProjectStatus",
        contentType: 'application/json; charset=utf-8',
        data: '{ "ProjectId": ' + ProjectId + '  }',
        dataType: 'json',
        success: function(data) {
            $('#lblProjectStatus').text(data.ProjectStatus);
            if (data.SuggestionSupplierRef === 0) {

            } else if (data.PaymentPlan == 0) {


            } else {

                if (data.CommentPerson != "") {
                    $('#btnWaitingComment').removeClass("hidden");
                    $("a").addClass("disabled");

                } else if (data.ConfirmPerson != "") {
                    $('#btnWaitingConfirm').removeClass("hidden");
                    $("a").addClass("disabled");

                } else if (data.RejectPerson != "") {
                    $('#btnGetConfirm').removeClass("hidden");
                } else {
                    if (data.ConfirmedPersonel > 0) {
                        $('#btnOrderSend').removeClass("hidden");
                        $('#btnConfirmed').removeClass("hidden");
                    } else {
                        $('#btnGetComment').removeClass("hidden");
                        $('#btnGetConfirm').removeClass("hidden");
                    }
                }
            }

            $("#btnComment").removeClass("disabled");


        },
    });


}

function getProjectStatusForOrder() {

    var ProjectId = $('#ProjectId').val();

    $.ajax({
        type: "POST",
        url: baseUrl() + "/Ajax/getProjectStatusForOrder",
        contentType: 'application/json; charset=utf-8',
        data: '{ "ProjectId": ' + ProjectId + '  }',
        dataType: 'json',
        success: function(data) {
            if (data != "") {
                $("#btnSendSupplier").addClass("hidden");
                $("#btnSendAsOrder").addClass("hidden");
                $("#btnSaveOrder").addClass("hidden");

                $('#lblProjectStatus').text(data);

            } else {
                $("#lblProjectStatus").addClass("hidden");
            }

        },
    });

}

function getPayPlan(ProjectId) {

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getPayPlan",
        data: '{ "ProjectId": ' + ProjectId + ' }',
        success: function(data) {
            setPayPlanData(data);


        },
        error: function(data) {

        }
    });
}

function getProjectList() {
    var BegDate = $('#begdate').val();
    var EndDate = $('#enddate').val();
    var supplier = $('#supplier').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/setProjectListFilter",
        data: '{ "BegDate": "' + BegDate + '","EndDate": "' + EndDate + '" ,"supplier": "' + supplier + '"}',
        success: function(data) {
            window.location.href = window.location.href;


        },
        error: function(data) {

        }
    });
}


function setPayPlanData(data) {
    var rowCount = data.length;
    for (var i = 0; i < rowCount; i++) {

        if (data[i].LineNr === 1) {
            $('#txtOdemeTutari1').val(data[i].AmountStr);
        } else if (data[i].LineNr === 2) {
            $('#txtOdemeTutari2').val(data[i].AmountStr);
        } else if (data[i].LineNr === 3) {
            $('#txtOdemeTutari3').val(data[i].AmountStr);
        } else if (data[i].LineNr === 4) {
            $('#txtOdemeTutari4').val(data[i].AmountStr);
        } else if (data[i].LineNr === 5) {
            $('#txtOdemeTutari5').val(data[i].AmountStr);
        } else if (data[i].LineNr === 6) {
            $('#txtOdemeTutari6').val(data[i].AmountStr);
        }
    }
}

function savePayPlan(ProjectId) {

    var amount1 = parseFloat($('#txtOdemeTutari1').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));
    var amount2 = parseFloat($('#txtOdemeTutari2').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));
    var amount3 = parseFloat($('#txtOdemeTutari3').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));
    var amount4 = parseFloat($('#txtOdemeTutari4').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));
    var amount5 = parseFloat($('#txtOdemeTutari5').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));
    var amount6 = parseFloat($('#txtOdemeTutari6').val().replace('.', '').replace('.', '').replace('.', '').replace(',', '.'));

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/savePlan",
        data: '{ "ProjectId": ' + ProjectId + ', "amount1": ' + amount1 + ',"amount2": ' + amount2 + ',"amount3": ' + amount3 + ',"amount4": ' + amount4 + ',"amount5": ' + amount5 + ',"amount6": ' + amount6 + ' }',
        success: function(data) {
            setPayPlanData(data);


        },
        error: function(data) {

        }
    });
}

function getHistory(projectId) {
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getHistory",
        data: '{ "projectId": "' + projectId + '"}',
        success: function(data) {

            var html = successHandlerHistory(data);

            $("#HistoryTableBody").html(html);

        },
        error: function(data) {

        }
    });




    $('#ModalHistory').modal('show');

}

function successHandlerHistory(data) {
    var rowCount = data.length;


    var html = "";

    for (var i = 0; i < rowCount; i++) {

        html = html + "<tr>";

        html = html + "<td>" + data[i].Person + "</td>";
        html = html + "<td>" + data[i].DateStr + "</td>";
        html = html + "<td>" + data[i].TimeStr + "</td>";
        html = html + "<td>" + data[i].Duration + "</td>";
        html = html + "<td>";
        html = html + data[i].ConfirmType;
        html = html + "</td>";
        html = html + "</tr>";
    }

    return html;
}

function removeOffers(offerNr) {

    var r = prompt(offerNr.toString() + ". Teklifler Silinecektir. Aciklama Giriniz:", "");
    if (r == null || r == "") {

    } else {
        var projectId = $('#ProjectId').val();
        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Ajax/removeOffers",
            data: '{ "offerNr": ' + offerNr + ', "projectId": ' + projectId + ',"Exp": "' + r + '"}',
            success: function(data) {
                window.location.href = data;
            },
            error: function(data) {

            }
        });
    }
}

function removeOffer(offerNr, supplierId) {
    var r = prompt("Tedarikcinin " + offerNr.toString() + ". Teklifi Silinecektir. Aciklama Giriniz:", "");
    if (r == null || r == "") {

    } else {

        var projectId = $('#ProjectId').val();
        $.ajax({
            type: 'Post',
            contentType: "application/json;",
            dataType: "json",
            url: baseUrl() + "/Ajax/removeOffer",
            data: '{ "offerNr": ' + offerNr + ', "supplierId": ' + supplierId + ', "projectId": ' + projectId + ',"Exp": "' + r + '"}',
            success: function(data) {
                window.location.href = data;
            },
            error: function(data) {

            }
        });
    }
}

function getProjectOnOrder() {
    var PrjNo = $('#txtSearch_PrjNo').val();
    var OrderNr = $('#txtSearch_OrderNr').val();
    var Supplier = $('#txtSearch_Supplier').val();
    var EndDate = $('#search_ProjectEndDate').val();
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getProjectOnOrder",
        data: '{ "PrjNo": "' + PrjNo + '", "OrderNr": "' + OrderNr + '", "Supplier": "' + Supplier + '","EndDate": "' + EndDate + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });
}



function getOfferExplain(requestGuid) {
    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getOfferExplain",
        data: '{ "requestGuid": "' + requestGuid + '"}',
        success: function(data) {
            alert(data);
        },
        error: function(data) {

        }
    });
}

function showOfferExplain(respondMessage) {
    alert(respondMessage);


}

function uploadProjectFile(projectId) {
    var fd = new FormData();
    var files = $('#projectFile')[0].files[0];
    fd.append('file', files);
    fd.append('ProjectId', projectId);
    $.ajax({
        url: baseUrl() + "/Demands/UploadFile",
        type: 'Post',
        data: fd,
        contentType: false,
        processData: false,
        success: function(data) {

            var rowCount = data.Docs.length;

            if (data.Success) {
                var html = "";

                for (var i = 0; i < rowCount; i++) {

                    html = html + " <tr> ";
                    html = html + "<td width=\"35%\">" + data.Docs[i].UploadedFileName + " </td> ";
                    html = html + "<td width=\"30%\">";
                    html = html + "<input class=\"form-control hidden\" data-val=\"true\" data-val-number=\"The field Id must be a number.\" data-val-required=\"Id alaný gereklidir.\" id=\"DocList_" + i + "__Id\" name=\"DocList[" + i + "].Id\" type=\"number\" value=\"" + data.Docs[i].Id + "\">";
                    html = html + "<input class=\"form-control\" id=\"DocList_" + i + "__DocumentName\" name=\"DocList[" + i + "].DocumentName\" style=\"font-weight: bold;font-size:12px !Important;width:100% !important;\" type=\"text\" value=\"\">";
                    html = html + "</td>";
                    html = html + "<td width=\"18%\">";
                    html = html + "<a role=\"button\" onclick=\"openAWindow('Demands/FileView?DocumentId=" + data.Docs[i].Id + "', 'Doküman Ýzleme' , 920, 640, true)\"  class=\"btn btn-info\"><i class=\"fa fa-street-view\"> </i> Ýncele</a>";
                    html = html + "</td>";
                    html = html + "<td width=\"17%\">";
                    html = html + "<a role=\"button\" onclick=\"SaveDocumentName('" + i + "')\" s class=\"btn btn-success\"><i class=\"fa fa-save\"> </i> Kaydet</a>";
                    html = html + "</td>";
                    html = html + "<td width=\"10%\">";
                    html = html + "<a role=\"button\" onclick=\"DeleteDocument('" + i + "')\" st class=\"btn btn-danger\"><i class=\"fa fa-remove\"></i> Sil</a>";
                    html = html + "</td>";
                    html = html + "</tr>";


                }
                $("#OfferDocumentTableBody").html(html);
            }
            if (data.Success) {
                $("input[name='txtUploadResult']").css('background-color', 'lightgreen');
            } else {
                $("input[name='txtUploadResult']").css('background-color', 'coral');
            }

            $("input[name='txtUploadResult']").val(data.Result);
        },
    });
}
function uploadOrderProjectFile(projectId) {
    var fd = new FormData();
    var files = $('#projectFile')[0].files[0];
    fd.append('file', files);
    fd.append('ProjectId', projectId);
    $.ajax({
        url: baseUrl() + "/Order/UploadFile",
        type: 'Post',
        data: fd,
        contentType: false,
        processData: false,
        success: function(data) {

            var rowCount = data.Docs.length;

            if (data.Success) {
                var html = "";

                for (var i = 0; i < rowCount; i++) {

                    html = html + " <tr> ";
                    html = html + "<td width=\"35%\">" + data.Docs[i].UploadedFileName + " </td> ";
                    html = html + "<td width=\"30%\">";
                    html = html + "<input class=\"form-control hidden\" data-val=\"true\" data-val-number=\"The field Id must be a number.\" data-val-required=\"Id alaný gereklidir.\" id=\"DocList_" + i + "__Id\" name=\"DocList[" + i + "].Id\" type=\"number\" value=\"" + data.Docs[i].Id + "\">";
                    html = html + "<input class=\"form-control\" id=\"DocList_" + i + "__DocumentName\" name=\"DocList[" + i + "].DocumentName\" style=\"font-weight: bold;font-size:12px !Important;width:100% !important;\" type=\"text\" value=\"\">";
                    html = html + "</td>";
                    html = html + "<td width=\"18%\">";
                    html = html + "<a role=\"button\" onclick=\"openAWindow('Order/FileView?DocumentId=" + data.Docs[i].Id + "', 'Doküman Ýzleme' , 920, 640, true)\"  class=\"btn btn-info\"><i class=\"fa fa-street-view\"> </i> Ýncele</a>";
                    html = html + "</td>";
                    html = html + "<td width=\"17%\">";
                    html = html + "<a role=\"button\" onclick=\"SaveOrderDocumentName('" + i + "')\"  class=\"btn btn-success\"><i class=\"fa fa-save\"> </i> Kaydet</a>";
                    html = html + "</td>";
                    html = html + "<td width=\"10%\">";
                    html = html + "<a role=\"button\" onclick=\"DeleteOrderDocument('" + i + "')\"  class=\"btn btn-danger\"><i class=\"fa fa-remove\"></i> Sil</a>";
                    html = html + "</td>";
                    html = html + "</tr>";


                }
                $("#OrderDocumentTableBody").html(html);
            }
            if (data.Success) {
                $("input[name='txtUploadResult']").css('background-color', 'lightgreen');
            } else {
                $("input[name='txtUploadResult']").css('background-color', 'coral');
            }

            $("input[name='txtUploadResult']").val(data.Result);
        },
    });
}

function getProjectsCompleted() {

    var PrjNo = $('#txtSearch_PrjNo').val();
    var OrderNr = $('#txtSearch_OrderNr').val();
    var Supplier = $('#txtSearch_Supplier').val();
    var EndDate = $('#search_ProjectEndDate').val();
    var InvoiceNr = $('#txtSearch_InvoiceNr').val();

    $.ajax({
        type: 'Post',
        contentType: "application/json;",
        dataType: "json",
        url: baseUrl() + "/Ajax/getProjectsCompleted",
        data: '{ "PrjNo": "' + PrjNo + '", "OrderNr": "' + OrderNr + '", "Supplier": "' + Supplier + '","InvoiceNr": "' + InvoiceNr + '","EndDate": "' + EndDate + '"}',
        success: function(data) {
            window.location.href = data;
        },
        error: function(data) {

        }
    });
}