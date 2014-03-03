function getPasswordHash(passwordElement, nonceElement, hashElement) {
    //get the password and the random value
    var password = $('#' + passwordElement).attr('value');
    var nonce = $('#' + nonceElement).attr('value');
    //fill in the hash with password & random value together
    $('#' + hashElement).attr('value', $.sha256(password + nonce));
    //delete the password so its not sent out
    $('#' + passwordElement).attr('value', '');
}