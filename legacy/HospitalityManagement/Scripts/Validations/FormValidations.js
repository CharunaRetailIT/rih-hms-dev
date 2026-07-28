function validateInputForNumbers(evt) {
   // debugger;
    var theEvent = evt || window.event;

    // Handle paste
    if (theEvent.type === 'paste') {
        key = event.clipboardData.getData('text/plain');
    } else {
        // Handle key press
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
    }
    var regex = /[0-9]|\./;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    }
}




function checkCodeTypeInput(event)
{
    if (
        !((event.keyCode >= 65) && (event.keyCode <= 90) ||
        (event.keyCode >= 97) && (event.keyCode <= 122) ||
        (event.keyCode >= 48) && (event.keyCode <= 57))
        )
        {
        event.returnValue = false;
        return;
        }
        event.returnValue = true;
}


function checkNameTypeInput(event)
{
    if (event.keyCode != 32)  
    {
        if (event.keyCode != 64) {

            if (!((event.keyCode >= 65) && (event.keyCode <= 90) || (event.keyCode >= 97) && (event.keyCode <= 122) ||
                (event.keyCode >= 48) && (event.keyCode <= 57))) {

                event.returnValue = false;
                return;

            }
            event.returnValue = true;
        }
    }else
    {
        event.returnValue=true;
    }
}

function validateEmailTypeInput(event)
{
    if (event.keyCode != 64)
    {

        if (!((event.keyCode >= 65) && (event.keyCode <= 90) || (event.keyCode >= 97) && (event.keyCode <= 122) ||
            (event.keyCode >= 48) && (event.keyCode <= 57))) {

            event.returnValue = false;
            return;

        }
        event.returnValue = true;
    } else {
        eve.returnValue = true;
    }
}

function preventTypeMinus(event) {
    debugger;
    if (event.keyCode != 32)
    {
        if (event.keyCode != 64)
        {

            if (!((event.keyCode >= 65) && (event.keyCode <= 90) || (event.keyCode >= 97) && (event.keyCode <= 122) ||
                (event.keyCode >= 48) && (event.keyCode <= 57))) {

                event.returnValue = false;
                return;

            }
            event.returnValue = true;
        }

    } else
    {
        event.returnValue = true;
    }
}