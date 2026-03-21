<?xml version="1.0" encoding="UTF-8"?>
<StyledLayerDescriptor version="1.0.0"
  xsi:schemaLocation="http://www.opengis.net/sld StyledLayerDescriptor.xsd"
  xmlns="http://www.opengis.net/sld"
  xmlns:ogc="http://www.opengis.net/ogc"
  xmlns:xlink="http://www.w3.org/1999/xlink"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <NamedLayer>
    <Name>spaces_ui_style</Name>
    <UserStyle>
      <Title>Espacios UI Ada Byron</Title>
      <FeatureTypeStyle>

        <Rule>
          <Title>Aula</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>AULA</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#3b82f6</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#1e3a8a</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Laboratorio</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>LABORATORIO</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#ef4444</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#7f1d1d</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Sala informática</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>SALA_INFORMATICA</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#0ea5e9</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#075985</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Sala reuniones</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>SALA_REUNIONES</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#8b5cf6</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#5b21b6</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Salón de actos</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>SALON_ACTOS</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#1d4ed8</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#1e3a8a</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Biblioteca</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>BIBLIOTECA</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#2563eb</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#1d4ed8</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Despacho</Title>
          <ogc:Filter>
            <ogc:PropertyIsEqualTo>
              <ogc:PropertyName>display_category</ogc:PropertyName>
              <ogc:Literal>DESPACHO</ogc:Literal>
            </ogc:PropertyIsEqualTo>
          </ogc:Filter>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#a855f7</CssParameter>
              <CssParameter name="fill-opacity">0.65</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#581c87</CssParameter>
              <CssParameter name="stroke-width">0.9</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

        <Rule>
          <Title>Contexto</Title>
          <ElseFilter/>
          <PolygonSymbolizer>
            <Fill>
              <CssParameter name="fill">#ffffff</CssParameter>
              <CssParameter name="fill-opacity">0.9</CssParameter>
            </Fill>
            <Stroke>
              <CssParameter name="stroke">#cbd5e1</CssParameter>
              <CssParameter name="stroke-width">0.6</CssParameter>
            </Stroke>
          </PolygonSymbolizer>
        </Rule>

      </FeatureTypeStyle>
    </UserStyle>
  </NamedLayer>
</StyledLayerDescriptor>
