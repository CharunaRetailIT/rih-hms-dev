
function checkGRNQty(i) {

    debugger;


    var pid = document.getElementById('TOGDetail[' + i + ']_ProductID').value;  
    var qty = document.getElementById('TOGDetail[' + i + ']_OrderQty').value;
     
    if (qty == "" || qty == null)
    {
        qty = 0;
    }

    var url = '/PO/CheckPrices';

            $.ajax({                
                url:url,
                data: { id: pid, qty: qty, polocid: $("#FromLocationId").val(), grnid: $("#GRNId").val() },
                type: "GET",
                dataType: "json",
                success: function (resp) 
                {
                    debugger;
                    data = resp;
                    
                    var actquantity = (data.OrderQuantity - data.TOGQuantity);
                   
                    if (actquantity < qty)
                    {
                        alert("Invalid Quantity!");
                        return;
                    }

                    document.getElementById('TOGDetail[' + i + ']_CostPrice').value = data.CostValue;
                    document.getElementById('TOGDetail[' + i + ']_SellingPrice').value = data.SellingPrice;

                    var table = document.getElementById("togitems");

                    document.getElementById("TotCostPrice").value = 0;
                    document.getElementById("TotSellingPrice").value = 0;
                 
                    var cp = 0;
                    var sp = 0;
                    var tp = 0;
                    for (var r = 0, n = table.rows.length; r < n; r++) {
                        cp += Number(document.getElementById('TOGDetail[' + r + ']_CostPrice').value);
                        sp += Number(document.getElementById('TOGDetail[' + r + ']_SellingPrice').value);

                    }

                    document.getElementById("TotSellingPrice").value = parseFloat(sp).toFixed(2);
                    document.getElementById("TotCostPrice").value = parseFloat(cp).toFixed(2);
                

                    document.getElementById("GrossAmount").value = parseFloat(
                        Number(document.getElementById("TotSellingPrice").value)
                    ).toFixed(2);

                    document.getElementById("NetAmount").value = parseFloat((
                            Number(document.getElementById("TotCostPrice").value))
                    ).toFixed(2);


                }

            });

}

