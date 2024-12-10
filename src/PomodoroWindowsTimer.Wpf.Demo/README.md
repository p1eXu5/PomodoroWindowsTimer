1. GlossOnBase: `ContentControl`
  - чилдрен окна
  - первоисточник о радиальном браше, зависящем от актуальных размеров

2. Beam
  - при инициализации ищет в визуальном дереве GlossOnBase: `ContentControl`
  - определяет своё смещение относительно него:

  ```cs
  Point beamPosition = beam.TransformToAncestor(glossOnBase).Transform(new Point(0, 0));
  ```
  - создаёт браш взяв параметры браша GlossOnBase