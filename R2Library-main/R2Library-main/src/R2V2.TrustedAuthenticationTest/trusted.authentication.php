<html>
<head>
<title>A Basic Implementation of Trusted Authentication</title>
</head>
<body>

<form action="" method="post" name="registerForm">

        <div>
            Please Enter your Authentication Key: 
            <input name="authenticationKey" type="text" value="" style="width: 300px" />
            <input style="width: 170px; height: 30px" type="submit" name="submit" value="Get Link To Home Page" />
        </div>               
        
</form>



<?php
if(isset($_POST['submit'])) {
	$authenticationKey = $_POST["authenticationKey"];
	
	$r2RequestString = 'http://www.r2library.com/TrustedAuthentication/?authenticationKey=%s';
	$r2RequestLink =  sprintf($r2RequestString, $authenticationKey);
	
	
	$html = implode('', file($r2RequestLink));

	echo "<a target='_blank' href='http://www.r2library.com/?" . urldecode($html) . "'>link to www.r2library.com </a><br />";

	
} 
?>


</body>
</html>