

    $.getJSON("@Url.Action('GetPromotionItems', 'ProductXYPromotions')", function (data) {

        var ProductId = $("#ProductId");

        ProductId.empty();
        ProductId.append("<option value='0'>-- Select a Product --</option>");

        $.each(JSON.parse(data), function (index, optionaldata) {
            ProductId.append($('<option />', { value: optionaldata.ProductId }).html(optionaldata.ProductName));
        });      
    });


