param(
    [Parameter(Mandatory=$true)]
    [string]$Prompt
)

# Copilot Ask-mode wrapper: repo Q&A only (read-only, no shell/write/url/memory)
copilot -s --no-ask-user --allow-all-tools `
    --deny-tool 'shell' `
    --deny-tool 'write' `
    --deny-tool 'url' `
    --deny-tool 'memory' `
    -p $Prompt
