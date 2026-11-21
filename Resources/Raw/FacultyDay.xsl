<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="UTF-8" indent="yes" />
  <xsl:template match="/">
    <html lang="uk"><head><meta charset="utf-8"/><title>День факультету</title>
      <style>body{font-family:Arial,sans-serif;margin:32px;background:#f4f6fa;color:#202938}h1{color:#2457a7}.event{background:white;padding:18px;margin:14px 0;border-radius:10px;border:1px solid #dce3ee}.meta{color:#52627a}</style>
    </head><body><h1>План заходів до Дня факультету</h1>
      <p><xsl:value-of select="FacultyDay/@faculty"/> — <xsl:value-of select="FacultyDay/@year"/></p>
      <xsl:for-each select="FacultyDay/Event"><div class="event">
        <h2><xsl:value-of select="Name"/></h2>
        <p><xsl:value-of select="Description"/></p>
        <p class="meta"><b>Відповідальний:</b> <xsl:value-of select="Responsible"/> · <b>Час:</b> <xsl:value-of select="StartTime"/> · <b>Місце:</b> <xsl:value-of select="Location"/></p>
        <p><b>Кафедра:</b> <xsl:value-of select="Department"/> · <b>Посада:</b> <xsl:value-of select="Position"/></p>
        <p><b>Доручення:</b> <xsl:value-of select="Tasks"/></p>
        <p><b>Термін підготовки:</b> <xsl:value-of select="PreparationDeadline"/></p>
        <p><b>Характеристика:</b> <xsl:value-of select="Characteristics"/></p>
      </div></xsl:for-each>
    </body></html>
  </xsl:template>
</xsl:stylesheet>
