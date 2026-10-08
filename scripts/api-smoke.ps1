param(
    [string]$BaseUrl = 'http://localhost:8080',
    [string]$DemoPassword = 'lab4-demo-password'
)
$ErrorActionPreference = 'Stop'
$stamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email = "lab4.video.$stamp@researchhub.local"

function Json($obj) { $obj | ConvertTo-Json -Depth 10 }
function Call($method, $path, $body = $null, $token = $null) {
    $headers = @{}
    if ($token) { $headers.Authorization = "Bearer $token" }
    $params = @{ Method=$method; Uri="$BaseUrl$path"; Headers=$headers; ContentType='application/json' }
    if ($null -ne $body) { $params.Body = Json $body }
    Invoke-RestMethod @params
}

Write-Host 'GET health'
Call GET '/health/ready' | Out-Null

Write-Host 'POST register'
$auth = Call POST '/api/auth/register' @{ name='Lab4 Smoke User'; email=$email; password=$DemoPassword }
$token = $auth.accessToken
$userId = $auth.userId

Write-Host 'GET projects'
Call GET '/api/projects?page=1&pageSize=5' $null $token | Out-Null

Write-Host 'POST project'
$project = Call POST '/api/projects' @{ name="Smoke Project $stamp"; description='Lab4 API smoke'; ownerId=$userId } $token

Write-Host 'PUT project'
Call PUT "/api/projects/$($project.id)" @{ name="Smoke Project Updated $stamp"; description='Updated' } $token | Out-Null

Write-Host 'POST member user 1'
Call POST "/api/projects/$($project.id)/members" @{ userId=1; role='Member' } $token | Out-Null

Write-Host 'POST task'
$task = Call POST '/api/tasks' @{ projectId=$project.id; title='Smoke task'; description='Lab4'; assigneeId=1; status='New'; priority='High'; dueDate=$null } $token

Write-Host 'PATCH status'
Call PATCH "/api/tasks/$($task.id)/status" @{ status='InProgress' } $token | Out-Null

Write-Host 'PATCH priority'
Call PATCH "/api/tasks/$($task.id)/priority" @{ priority='Critical' } $token | Out-Null

Write-Host 'GET filtered tasks'
Call GET "/api/projects/$($project.id)/tasks?status=InProgress&priority=Critical&assigneeId=1&page=1&pageSize=10" $null $token | Out-Null

Write-Host 'Expected business error: duplicate member -> 409'
try {
    Call POST "/api/projects/$($project.id)/members" @{ userId=1; role='Member' } $token | Out-Null
    throw 'Expected 409 was not returned'
}
catch {
    if ($_.Exception.Response.StatusCode.value__ -ne 409) { throw }
    Write-Host '409 received as expected' -ForegroundColor Yellow
}

Write-Host 'DELETE task'
Invoke-WebRequest -Method DELETE -Uri "$BaseUrl/api/tasks/$($task.id)" -Headers @{Authorization="Bearer $token"} -UseBasicParsing | Out-Null
Write-Host 'DELETE project'
Invoke-WebRequest -Method DELETE -Uri "$BaseUrl/api/projects/$($project.id)" -Headers @{Authorization="Bearer $token"} -UseBasicParsing | Out-Null

Write-Host 'API SMOKE TEST PASSED (GET/POST/PUT/PATCH/DELETE + business error)' -ForegroundColor Green
