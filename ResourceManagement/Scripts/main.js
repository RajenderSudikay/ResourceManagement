jQuery(document).ready(function () {
    FindAncherTagInHtml();

    jQuery(".dropdownheading").click(function () {
        jQuery(".agent-description").toggleClass("show");
        jQuery(this).toggleClass("active");
    });

    //This logic to handle out side clik of agent popup
    jQuery(document).on('click', function (e) {
        if (jQuery(e.target).closest(".agent-navigation").length === 0) {
            jQuery(".agent-description").hide();
        }

        if (jQuery(e.target).closest(".agent-navigation").length > 0 && jQuery(".agent-description.show").length > 0) {
            jQuery(".agent-description").show();
        }
        else {
            jQuery(".agent-description").hide();
            jQuery(".dropdownheading").removeClass("active");
            jQuery(".agent-description").removeClass("show");
        }
    });

    jQuery('.noagent-search input').val('');

    jQuery('.noagent-search input').on('keyup', function () {
        var self = jQuery(this);
        if (self.val() != '') {
            jQuery(this).addClass('is-not-empty');
        } else {
            jQuery(this).removeClass('is-not-empty');
        }
    });

    jQuery('.noagent-search input').on('keypress', function (e) {
        var inputVal = jQuery(this).val();
        var reqVal = "/de-de/vor-ort?newsearch=true&navSearchTerm=" + inputVal;
        if (inputVal != '' && e.which == 13) {
            window.location = reqVal;
            return false;
        }
    });



    // quick links js
    function HideFlyout() {
        jQuery('.main-toggle-flyout').removeClass('active');
        jQuery('.main-toggle-flyout').find('.column-splitter').removeClass('show');
    }

    function FindAncherTagInHtml() {
        var findAncherTag = $('.dropdown-flyout').find('a');
        var separator = ".";
        $.each(findAncherTag, function (key, value) {
            var findClassName = findAncherTag[key].closest('.main-toggle-flyout').className;
            findClassName = separator + findClassName.replace(/ /g, separator);
            if (jQuery(findClassName).find('a').attr('href')) {
                jQuery(findClassName).addClass('removecss-mobile');
            }
        });
    }

    jQuery(".dropdown-flyout").click(function (e) {
        var mainContentDiv = jQuery(this).parent().parent();
        if (mainContentDiv.hasClass('active')) {
            HideFlyout();
        }
        else {
            if (!$(this).find('a').attr('href')) {
                HideFlyout();
                mainContentDiv.find(".column-splitter").toggleClass("show");
                mainContentDiv.toggleClass("active");
            }
        }
    });
    jQuery(document).on('click', function (e) {
        if (jQuery(e.target).closest(".main-toggle-flyout").length === 0) {
            HideFlyout();
        }
    });


    jQuery(document).ajaxSend(function () {
        jQuery("#overlay").fadeIn(300);
    });

    jQuery(document).ajaxComplete(function (event, request, set) {
        jQuery("#overlay").fadeOut(300);
    });
});

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

jQuery(document).ready(function () {
    if (window.location.href.toString().toLowerCase().indexOf("?success") > -1) {
        var responseSuccess = getUrlVars()["success"];
        var selectedEmp = decodeURIComponent(getUrlVars()["selectedemp"]);

        if (responseSuccess == "true") {
            jQuery('.success-message-popup-title').html("Hey there");
            jQuery('.database-response').html("<h2><b> " + selectedEmp + "</b> details have been successfully saved, and we greatly appreciate your cooperation.</h2>");
            jQuery('.message-success-popup').modal('show');
        }
        else {
            var responseError = getUrlVars()["error"];
            var decodedUrlText = decodeURIComponent(responseError);
            jQuery('.error-message-popup-title').html("Hey there");
            jQuery('.database-response').html("<h2>" + decodedUrlText + "</h2>");
            jQuery('#modal-db-error-alert').modal('show');
        }

        var yourCurrentUrl = window.location.href.split('?')[0];
        window.history.replaceState({}, '', yourCurrentUrl);
    }

});


jQuery('textarea').on('input', function () {
    this.style.height = 'auto';

    this.style.height =
        (this.scrollHeight) + 'px';
});

function GetAssetsByEmpID(empID) {
    var GetAssetEmpModel = {
        EmpID: empID,
    };

    jQuery.ajax({
        type: "POST",
        url: "/it/getAssetsbyempid",
        data: JSON.stringify({ GetAssetModelByEmp: GetAssetEmpModel }),
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            jQuery('#assetAssignID').empty();
            jQuery.each(data.EmpSpecificAssets, function (i, item) {            
                $('#assetAssignID').append('<option value="' + item.AssetSerialNo + '">' + item.AssetSerialNo + '</option>');
            });

            if (jQuery('#assetAssignID :selected').val() == "") {
                jQuery('#assetAssignID').css("border", "1px solid red");
                jQuery('.invalid-assetid').show();
            }
            else {
                jQuery('#assetAssignID').css("border", "1px solid green");
                jQuery('.invalid-assetid').hide();
            }
        }
    });
}

function GetAssetsByAssetType(type) {
    var GetAssetModel = {
        AssetType: type,
    };

    jQuery.ajax({
        type: "POST",
        url: "/it/GetAssetsByAssetType",
        data: JSON.stringify({ GetAssetModel: GetAssetModel }),
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            jQuery('#assetAssign').empty();
            $('#assetAssign').append('<option value="">Select Asset</option>');
            jQuery.each(data.AssetsByAssetType, function (i, item) {
                $('#assetAssign').append('<option value="' + item.AssetSerialNo + '">' + item.AssetSerialNo + '</option>');
            });           
        }
    });
}

function GenerateGUID() {
    let u = Date.now().toString(16) + Math.random().toString(16) + '0'.repeat(16);
    let guid = [u.substr(0, 8), u.substr(8, 4), '4000-8' + u.substr(13, 3), u.substr(16, 12)].join('-');
    return guid;
}


/*    jQuery("#selectEmployeenameandemail").change(function (evt) {*/
jQuery(document).on("change", "#selectEmployeenameandemail", function () {
    jQuery('.invalid-emp').hide();
    jQuery('#selectEmployeenameandemail').css("border", "1px solid green");
    GetAssetsByEmpID(jQuery('#selectEmployeenameandemail :selected').val().split('|')[0]);
});

/* jQuery("#assetAssignID").change(function (evt) {*/
jQuery(document).on("change", "#assetAssignID", function () {
    jQuery('.invalid-assetid').hide();
    jQuery('#assetAssignID').css("border", "1px solid green");
});

/*   jQuery("#assetUploadMonth").change(function (evt) {*/
jQuery(document).on("change", "#assetUploadMonth", function () {
    jQuery('.invalid-month').hide();
    jQuery('#assetUploadMonth').css("border", "1px solid green");
});


function AssetFormValidated()
{
    var isFormValidated = true;
    if (jQuery('#selectEmployeenameandemail :selected').val() == "") {
        jQuery('.invalid-emp').show();
        jQuery('#selectEmployeenameandemail').css("border", "1px red solid");
        isFormValidated = false;
    }
    else {
        jQuery('.invalid-emp').hide();
        jQuery('#selectEmployeenameandemail').css("border", "1px red green");
    }

    if (jQuery('#assetAssignID :selected').val() == "") {
        jQuery('.invalid-assetid').show();
        jQuery('#assetAssignID').css("border", "1px red solid");
        isFormValidated = false;
    }
    else {
        jQuery('.invalid-assetid').hide();
        jQuery('#assetAssignID').css("border", "1px red green");
    }

    if (jQuery('#assetUploadMonth :selected').val() == "") {
        jQuery('.invalid-month').show();
        jQuery('#assetUploadMonth').css("border", "1px red solid");
        isFormValidated = false;
    }
    else {
        jQuery('.invalid-month').hide();
        jQuery('#assetUploadMonth').css("border", "1px red green");
    }

    return isFormValidated;
}



