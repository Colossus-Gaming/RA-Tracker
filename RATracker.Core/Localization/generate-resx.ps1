# Regenerates the .resx catalogues from translations.tsv.
#
# The translations live in a separate UTF-8 data file rather than inline in this script on purpose:
# Windows PowerShell 5.1 reads a BOM-less UTF-8 *script* as ANSI, which silently mangles every
# accented character ("Anderungen" instead of "Änderungen") before it is ever written. Reading the
# data with an explicit -Encoding UTF8 avoids that entirely.
#
# Usage:  pwsh -File generate-resx.ps1

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$tsv = Join-Path $here 'translations.tsv'

$lines = Get-Content $tsv -Encoding UTF8 | Where-Object { $_.Trim() -ne '' }
$headers = $lines[0] -split "`t"
$rows = $lines[1..($lines.Count - 1)]

$schema = @'
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
'@

# Column 1 is the key; column 2 (en) is also the neutral fallback catalogue.
for ($col = 1; $col -lt $headers.Count; $col++) {
    $lang = $headers[$col].Trim()
    $suffix = if ($lang -eq 'en') { '' } else { ".$lang" }
    $path = Join-Path $here "Strings$suffix.resx"

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine('<root>')
    [void]$sb.AppendLine($schema)

    foreach ($row in $rows) {
        $cols = $row -split "`t"
        $key = $cols[0].Trim()
        $value = if ($cols.Count -gt $col -and $cols[$col].Trim() -ne '') { $cols[$col].Trim() } else { $cols[1].Trim() }
        $value = [System.Security.SecurityElement]::Escape($value)
        [void]$sb.AppendLine("  <data name=`"$key`" xml:space=`"preserve`"><value>$value</value></data>")
    }

    [void]$sb.AppendLine('</root>')

    # UTF-8 without BOM; the XML declaration above states the encoding.
    [System.IO.File]::WriteAllText($path, $sb.ToString(), (New-Object System.Text.UTF8Encoding $false))
    Write-Host ("wrote {0,-20} {1} keys" -f "Strings$suffix.resx", $rows.Count)
}
