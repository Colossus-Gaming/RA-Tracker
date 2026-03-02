param(
    [Parameter(Mandatory=$true)]
    [string]$Prompt
)

# Codex implementation wrapper: full-auto mode in current repo
codex exec --full-auto --cd . $Prompt
