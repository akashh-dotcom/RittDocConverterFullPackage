<html>
<head>
<title>A Basic Implementation of Trusted Authentication</title>
</head>
<body>

<?php
class WebTrustedAuthentication
{
	public $Timestamp;
	public $Hash;
	public $ErrorMessage;

	public function GetQueryStringParameters($accountNumber)
	{
		$format = 'timestamp=%s&acctno=%s&hash=%s';
		return sprintf($format, $this->Timestamp, $accountNumber, $this->Hash);
	}
	
	public function __construct( $Timestamp, $Hash, $ErrorMessage )
	{
		$this->Timestamp = $Timestamp;
		$this->Hash = $Hash;
		$this->ErrorMessage = $ErrorMessage;
	}

	public static function createFromJson( $jsonString )
	{
		$object = json_decode( $jsonString );
		
		return new self( $object->Timestamp, $object->Hash, $object->ErrorMessage );
	}
	
}
	
$authenticationKey = 'j5cpL0zxkLU9Y3MdG6YdhTEu';
$accountNumber = "005033";

$r2RequestString = 'http://dev.r2library.com/TrustedAuthentication/?authenticationKey=%s';
$r2RequestLink =  sprintf($r2RequestString, $authenticationKey);
$json = implode('', file($r2RequestLink));
	
	
$reconstructedWebTrustedAuthentication = WebTrustedAuthentication::createFromJson( $json );
	
if(isset($reconstructedWebTrustedAuthentication-> ErrorMessage))
{
	echo $reconstructedWebTrustedAuthentication-> ErrorMessage;
	return;
}
	
	
$queryStringParameters = $reconstructedWebTrustedAuthentication->GetQueryStringParameters($accountNumber);
	
$HomePageLink = sprintf('http://dev.r2library.com/?%s', $queryStringParameters);
	
$BookLink = sprintf('http://dev.r2library.com/resource/detail/1449640397/pr0003?%s',$queryStringParameters);
	
	
$anchorTag = "<a target='_blank' href='%s'>%s</a><br />";
	
	 
echo "Links to R2library.com";
echo "<br />"; 
echo sprintf($anchorTag,  $HomePageLink, "Home page");
echo "<br />"; 
echo sprintf($anchorTag,  $BookLink, "Into a Book");
	


?>

</body>
</html>