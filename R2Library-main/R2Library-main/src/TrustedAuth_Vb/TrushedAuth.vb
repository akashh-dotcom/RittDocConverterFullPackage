Imports System
Imports System.Text
Imports System.Security.Cryptography


Public Class TrushedAuth

    Public Shared Function GetHashKey(ByVal AccountNumber As String, ByVal SecretCode As String, ByVal Salt As String, ByVal Timestamp As DateTime) As String

        Dim stringToHash As New System.Text.StringBuilder()

        stringToHash.Append(AccountNumber).Append("|").Append(Timestamp).Append("|").Append(SecretCode)

        Dim encoding As New System.Text.ASCIIEncoding()
        Dim saltBytes() As Byte = encoding.GetBytes(Salt)

        Dim hashedString As String = ComputeSHA1Hash(stringToHash.ToString(), saltBytes)

        'Dim sha1Provider As New Security.Cryptography.SHA1CryptoServiceProvider

        'sha1Provider.

        'Dim bytesToHash() As Byte = System.Text.Encoding.ASCII.GetBytes(stringToHash.ToString())

        'bytesToHash = sha1Provider.ComputeHash(bytesToHash)

        'Dim strResult As String = ""

        'For Each b As Byte In bytesToHash
        '    strResult += b.ToString("x2")
        'Next

        'Return strResult

        Return hashedString

    End Function


    Public Shared Function ComputeSHA1Hash(ByVal plainText As String, ByVal saltBytes() As Byte) As String

        ' Convert plain text into a byte array.
        Dim plainTextBytes As Byte()
        plainTextBytes = Encoding.UTF8.GetBytes(plainText)

        ' Allocate array, which will hold plain text and salt.
        Dim plainTextWithSaltBytes() As Byte = _
            New Byte(plainTextBytes.Length + saltBytes.Length - 1) {}

        ' Copy plain text bytes into resulting array.
        Dim I As Integer
        For I = 0 To plainTextBytes.Length - 1
            plainTextWithSaltBytes(I) = plainTextBytes(I)
        Next I

        ' Append salt bytes to the resulting array.
        For I = 0 To saltBytes.Length - 1
            plainTextWithSaltBytes(plainTextBytes.Length + I) = saltBytes(I)
        Next I

        Dim hash As HashAlgorithm = New SHA1Managed()

        ' Compute hash value of our plain text with appended salt.
        Dim hashBytes As Byte() = hash.ComputeHash(plainTextWithSaltBytes)

        ' Create array which will hold hash and original salt bytes.
        Dim hashWithSaltBytes() As Byte = New Byte(hashBytes.Length + saltBytes.Length - 1) {}

        ' Copy hash bytes into resulting array.
        For I = 0 To hashBytes.Length - 1
            hashWithSaltBytes(I) = hashBytes(I)
        Next I

        ' Append salt bytes to the result.
        For I = 0 To saltBytes.Length - 1
            hashWithSaltBytes(hashBytes.Length + I) = saltBytes(I)
        Next I

        ' Convert result into a base64-encoded string.
        Dim hashValue As String
        hashValue = Convert.ToBase64String(hashWithSaltBytes)

        ' Return the result.
        ComputeSHA1Hash = hashValue
    End Function

End Class

