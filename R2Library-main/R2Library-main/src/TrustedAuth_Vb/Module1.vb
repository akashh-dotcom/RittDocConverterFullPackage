Module Module1

    Sub Main()
        Dim accountNumber As String = "123123"
        Dim salt As String = "23sdf453fas54519"   ' 16 character salt - the longer the better - 16 was selected for display purposed in this demo
        Dim secret As String = "orange"
        Dim timestamp As DateTime = DateTime.UtcNow

        ' this is the hash calculated with R2 based on the account number and time stamp in the reques and the values in the database
        Dim hashKeyR2 As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumber, secret, salt, timestamp)
        Console.WriteLine("R2 Hash:      " + hashKeyR2)

        ' this is the hash that would be calculated by the client. since all values are the same, the hash should match
        Dim hashKeyFromRequest As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumber, secret, salt, timestamp)
        Console.Write("Request Hash: " + hashKeyFromRequest)
        If (hashKeyFromRequest = hashKeyR2) Then
            Console.WriteLine(" - valid")
        Else
            Console.WriteLine(" - INVALID")
        End If

        Dim accountNumberInvalid As String = "123456"
        Dim saltInvalid As String = "fas5423sdf453--+"
        Dim secretInvalid As String = "Don't Tell Anyone!"
        Dim timestampInvalid As DateTime = DateTime.UtcNow.AddSeconds(33)

        ' example of an invalid account number in the hash
        Dim hashKeyBadRequestAccountNumber As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumberInvalid, secret, salt, timestamp)
        Console.Write("Bad Hash #1:  " + hashKeyBadRequestAccountNumber)
        If (hashKeyBadRequestAccountNumber = hashKeyR2) Then
            Console.WriteLine(" - valid")
        Else
            Console.WriteLine(" - INVALID")
        End If

        ' example of an invalid salt value
        Dim hashKeyBadRequestSalt As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumber, secret, saltInvalid, timestamp)
        Console.Write("Bad Hash #2:  " + hashKeyBadRequestSalt)
        If (hashKeyBadRequestSalt = hashKeyR2) Then
            Console.WriteLine(" - valid")
        Else
            Console.WriteLine(" - INVALID")
        End If

        ' example of an invalid secret value
        Dim hashKeyBadRequestSecret As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumber, secretInvalid, salt, timestamp)
        Console.Write("Bad Hash #3:  " + hashKeyBadRequestSecret)
        If (hashKeyBadRequestSecret = hashKeyR2) Then
            Console.WriteLine(" - valid")
        Else
            Console.WriteLine(" - INVALID")
        End If

        ' example of an invalid timestamp
        Dim hashKeyBadRequestTimestamp As String = TrustedAuthPrototype.TrushedAuth.GetHashKey(accountNumber, secret, salt, timestampInvalid)
        Console.Write("Bad Hash #4:  " + hashKeyBadRequestTimestamp)
        If (hashKeyBadRequestTimestamp = hashKeyR2) Then
            Console.WriteLine(" - valid")
        Else
            Console.WriteLine(" - INVALID")
        End If


    End Sub

End Module
