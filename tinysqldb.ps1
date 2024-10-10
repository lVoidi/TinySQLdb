param (
    [Parameter(Mandatory = $true)]
    [string]$IP,
    [Parameter(Mandatory = $true)]
    [string]$PATH,
    [Parameter(Mandatory = $true)]
    [int]$Port
)

$ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse("127.0.0.1"), 11000)

function Send-Message {
    param (
        [Parameter(Mandatory=$true)]
        [pscustomobject]$message,
        [Parameter(Mandatory=$true)]
        [System.Net.Sockets.Socket]$client
    )

    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $writer = New-Object System.IO.StreamWriter($stream)
    try {
        $writer.WriteLine($message)
    }
    finally {
        $writer.Close()
        $stream.Close()
    }
}

function Receive-Message {
    param (
        [System.Net.Sockets.Socket]$client
    )
    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $reader = New-Object System.IO.StreamReader($stream)
    try {
        $message = $reader.ReadLine()
        if ($null -ne $message) {
            return $message
        } else {
            return ""
        }
    }
    finally {
        $reader.Close()
        $stream.Close()
    }
}
function Send-SQLCommand {
    param (
        [string]$filePath
    )
    
    # Read the file content
    $command = Get-Content -Path $filePath -Raw | Out-String
    
    $client = New-Object System.Net.Sockets.Socket($ipEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
    $client.Connect($ipEndPoint)
    $requestObject = [PSCustomObject]@{
        Type = 0;
        Body = $command
    }
    Write-Host -ForegroundColor Green "Sending command: $command"

    $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress
    Send-Message -client $client -message $jsonMessage
    $response = Receive-Message -client $client

    Write-Host -ForegroundColor Green "Response received: $response"
    
    if (-not [string]::IsNullOrWhiteSpace($response)) {
        $responseObject = ConvertFrom-Json -InputObject $response
        if ($responseObject.PSObject.Properties.Name -contains 'Status') {
            if ($responseObject.Status -eq '1') {
                Write-Host -ForegroundColor Red $responseObject.Status
            } else {
                Write-Host -ForegroundColor Green $responseObject.Status
            }
        } else {
            Write-Warning "El campo 'Status' no está presente en la respuesta JSON."
        }
    }
    
    $client.Shutdown([System.Net.Sockets.SocketShutdown]::Both)
    $client.Close()
}

Send-SQLCommand -filePath $PATH
