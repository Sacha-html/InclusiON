# generar-metricas.ps1
# Genera metricas-todos-los-sprints.md desde Jira-CSV.csv

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$csvPath   = Join-Path $scriptDir 'Jira-CSV.csv'
$outPath   = Join-Path $scriptDir 'metricas-todos-los-sprints.md'
$checklistPath = Join-Path (Split-Path $scriptDir -Parent) 'State\checklist-procesos.md'

$lines = Get-Content $csvPath

# Detect Sprint column index dynamically from header
$header = $lines[0] -split ',(?=(?:[^"]*"[^"]*")*[^"]*$)'
$sprintIdx   = ($header | Select-String -SimpleMatch 'Sprint' | Select-Object -First 1).LineNumber - 1
$statusIdx   = 4; $typeIdx = 3; $assigneeIdx = 13
$issueKeyIdx = 1; $summaryIdx = 0
Write-Host "Sprint column: $sprintIdx ($($header[$sprintIdx]))"

$sprints = [ordered]@{}
for ($i = 1; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Trim() -eq '') { continue }
    $cols = $lines[$i] -split ',(?=(?:[^"]*"[^"]*")*[^"]*$)'
    if ($cols.Count -le $sprintIdx) { continue }
    $sprint = $cols[$sprintIdx].Trim().Trim('"')
    if ($sprint -eq '' -or $sprint -eq '(Sin sprint)') { continue }
    if (-not $sprints.Contains($sprint)) { $sprints[$sprint] = [System.Collections.ArrayList]@() }
    [void]$sprints[$sprint].Add([PSCustomObject]@{
        Key      = $cols[$issueKeyIdx].Trim().Trim('"')
        Summary  = $cols[$summaryIdx].Trim().Trim('"')
        Status   = $cols[$statusIdx].Trim().Trim('"')
        Type     = $cols[$typeIdx].Trim().Trim('"')
        Assignee = $cols[$assigneeIdx].Trim().Trim('"')
    })
}

# Statuses that count as completed
$completedStatuses = @('Done', 'For Review', 'Awaiting Feedback')

# Label map: Jira status → display label
$statusLabels = @{
    'For Review'        = 'Desarrollada ✓'
    'Awaiting Feedback' = 'Desarrollada ✓ (esperando feedback)'
    'Done'              = 'Completada ✓'
    'In Progress'       = 'En progreso'
    'Backlog'           = 'Pendiente'
}

# Parse checklist: extract all IN-XXX numbers that have [x]
$checklistKeys = @{}
if (Test-Path $checklistPath) {
    $checklistContent = Get-Content $checklistPath -Raw
    # Find section headers with IN-numbers: "> Ruta: ... · IN-21, IN-22"
    $sectionMatches = [regex]::Matches($checklistContent, '>\s+Ruta:.*?·\s+(IN-[\d, IN-]+)')
    foreach ($m in $sectionMatches) {
        $inNums = [regex]::Matches($m.Groups[1].Value, 'IN-\d+')
        foreach ($n in $inNums) { $checklistKeys[$n.Value] = $true }
    }
    # Also grab inline IN-XXX references in checked items [x]
    $checkedLines = $checklistContent -split "`n" | Where-Object { $_ -match '^\s*-\s+\[x\]' }
    foreach ($line in $checkedLines) {
        $inMatches = [regex]::Matches($line, 'IN-\d+')
        foreach ($n in $inMatches) { $checklistKeys[$n.Value] = $true }
    }
}

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('# Métricas de Sprints — InclusiON')
[void]$sb.AppendLine()
[void]$sb.AppendLine('> Generado automáticamente desde Jira-CSV.csv')
[void]$sb.AppendLine("> Fecha: $(Get-Date -Format 'yyyy-MM-dd')")
[void]$sb.AppendLine()
[void]$sb.AppendLine('**Convención de estados:**')
[void]$sb.AppendLine()
[void]$sb.AppendLine('| Estado Jira | Significado |')
[void]$sb.AppendLine('|-------------|-------------|')
[void]$sb.AppendLine('| **Desarrollada ✓** | HU implementada y funcional. Código en producción. |')
[void]$sb.AppendLine('| **Completada ✓** | HU aprobada formalmente. |')
[void]$sb.AppendLine('| En progreso | Desarrollo en curso. |')
[void]$sb.AppendLine('| Pendiente | En backlog, no iniciada. |')
[void]$sb.AppendLine()
[void]$sb.AppendLine('> ✅ = verificado en checklist de procesos (`State/checklist-procesos.md`)')
[void]$sb.AppendLine()
[void]$sb.AppendLine('---')
[void]$sb.AppendLine()

# Sprint order mapping
$sprintOrder = @{
    'Tablero Sprint 0'              = 0
    'Sprint 1 (Config de sistema)'  = 1
    'Sprint 2 (Gestion de usuarios)'= 2
    'Tablero Sprint 3'              = 3
    'Tablero Sprint 4'              = 4
    'Tablero Sprint 5'              = 5
    'Tablero Sprint 6'              = 6
    'Tablero Sprint 7'              = 7
    'Tablero Sprint 8 (En curso)'   = 8
    'Tablero Sprint 8'              = 8
    'Tablero Sprint 9'              = 9
    'Tablero Sprint 9 (En Curso)'   = 9
}

$sprintNames = @{
    'Tablero Sprint 0'              = 'Sprint 0 — Arranque'
    'Sprint 1 (Config de sistema)'  = 'Sprint 1 — Configuración del Sistema'
    'Sprint 2 (Gestion de usuarios)'= 'Sprint 2 — Gestión de Usuarios'
    'Tablero Sprint 3'              = 'Sprint 3 — Autenticación y Accesibilidad'
    'Tablero Sprint 4'              = 'Sprint 4 — Evaluación, Dashboard y Actividades'
    'Tablero Sprint 5'              = 'Sprint 5 — Reportes'
    'Tablero Sprint 6'              = 'Sprint 6 — Seguridad y Features Avanzadas'
    'Tablero Sprint 7'              = 'Sprint 7 — Actividades, Roadmap y Ejecución'
    'Tablero Sprint 8 (En curso)'   = 'Sprint 8 — Players y Seguimiento'
    'Tablero Sprint 8'              = 'Sprint 8 — Players y Seguimiento'
    'Tablero Sprint 9'              = 'Sprint 9 — Mensajería y Portal Familiar'
    'Tablero Sprint 9 (En Curso)'   = 'Sprint 9 — Mensajería y Portal Familiar (En Curso)'
}

$sortedSprints = $sprints.Keys | Sort-Object { if ($sprintOrder.ContainsKey($_)) { $sprintOrder[$_] } else { 99 } }

foreach ($sprintKey in $sortedSprints) {
    $items = $sprints[$sprintKey]
    $stories = $items | Where-Object { $_.Type -eq 'Story' }
    $tasks = $items | Where-Object { $_.Type -ne 'Story' }
    $completedStories = $stories | Where-Object { $completedStatuses -contains $_.Status }
    $completedTasks = $tasks | Where-Object { $completedStatuses -contains $_.Status }
    $displayName = if ($sprintNames.ContainsKey($sprintKey)) { $sprintNames[$sprintKey] } else { $sprintKey }

    [void]$sb.AppendLine("## $displayName")
    [void]$sb.AppendLine()

    # Resumen table
    $velocity = if ($stories.Count -gt 0) { [math]::Round($completedStories.Count * 100 / $stories.Count) } else { 0 }
    [void]$sb.AppendLine('### Resumen')
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('| Métrica | Valor |')
    [void]$sb.AppendLine('|---------|-------|')
    [void]$sb.AppendLine("| Total issues | $($items.Count) |")
    [void]$sb.AppendLine("| Historias de Usuario | $($stories.Count) |")
    [void]$sb.AppendLine("| HU completadas | $($completedStories.Count) |")
    [void]$sb.AppendLine("| Velocidad (HU) | $velocity% |")
    [void]$sb.AppendLine("| Tasks completadas | $($completedTasks.Count) / $($tasks.Count) |")
    [void]$sb.AppendLine()

    # Por estado
    [void]$sb.AppendLine('### Por estado')
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('| Estado | Cantidad |')
    [void]$sb.AppendLine('|--------|----------|')
    $items | Group-Object Status | Sort-Object Name | ForEach-Object {
        $label = if ($statusLabels.ContainsKey($_.Name)) { $statusLabels[$_.Name] } else { $_.Name }
        [void]$sb.AppendLine("| $label | $($_.Count) |")
    }
    [void]$sb.AppendLine()

    # Por miembro
    [void]$sb.AppendLine('### Por miembro')
    [void]$sb.AppendLine()
    [void]$sb.AppendLine('| Miembro | Issues asignadas | HU completadas |')
    [void]$sb.AppendLine('|---------|-----------------|----------------|')
    $items | Group-Object Assignee | Sort-Object -Descending Count | ForEach-Object {
        $memberName = if ($_.Name -eq '') { '(Sin asignar)' } else { $_.Name }
        $memberItems = @($_.Group)
        $memberHU = ($memberItems | Where-Object { $_.Type -eq 'Story' -and $completedStatuses -contains $_.Status }).Count
        [void]$sb.AppendLine("| $memberName | $($_.Count) | $memberHU |")
    }
    [void]$sb.AppendLine()

    # HU list
    if ($stories.Count -gt 0) {
        [void]$sb.AppendLine('### Historias de Usuario')
        [void]$sb.AppendLine()
        $stories | Sort-Object { ($_.Key -replace '\D','') -as [int] } | ForEach-Object {
            $checkmark    = if ($completedStatuses -contains $_.Status) { 'x' } else { ' ' }
            $inChecklist  = if ($checklistKeys.ContainsKey($_.Key)) { ' ✅' } else { '' }
            $statusLabel  = if ($statusLabels.ContainsKey($_.Status)) { $statusLabels[$_.Status] } else { $_.Status }
            [void]$sb.AppendLine("- [$checkmark] **$($_.Key)**$inChecklist — $($_.Summary) _[$statusLabel]_")
        }
        [void]$sb.AppendLine()
    }

    [void]$sb.AppendLine('---')
    [void]$sb.AppendLine()
}

Set-Content -Path $outPath -Value $sb.ToString() -Encoding UTF8
Write-Host "Generado: $outPath"
