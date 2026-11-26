## Domen i prostorne osnove

- **Geo (`Geo.cs`)**: Centralizuje WGS84 (`SRID 4326`) `GeometryFactory` i `CreatePoint(lon, lat)` uz redoslijed `(X = lon, Y = lat)`.
- **SiteCategory (`SiteCategory.cs`)**: Kategorizacija kulturno-istorijskog naslijeđa (`Fortress`, `Bridge`, `Mosque`, `Church`, `Monastery`, `Necropolis`, `Archaeological`, `OldTown`, `Museum`, `Monument`, `Other`).
- **HeritageSite (`HeritageSite.cs`)**: Glavni entitet sa `Guid` identifikatorima generisanim na klijentu, istorijskim i prostornim metapodacima, NTS `Point` geometrijom (`Location`), tragom uvoza (`WikidataId`, `OsmType`, `OsmId`), `HeroImageUrl`, 3D modelom (`Model`) i galerijom (`Media`).
- **SiteMedia (`SiteMedia.cs`)**: Multimedijalni zapisi vezani za lokaciju, uključujući URL, opis te obavezne podatke o autorstvu i licenci.
- **SiteModel3D (`SiteModel3D.cs`)**: Metapodaci o 3D modelu (`.glb`/`.gltf` URL, `Scale`, `HeadingDeg`, `AltitudeOffsetM` i `FileSizeBytes` za procjenu lazy-loadinga).

## Aplikacioni sloj (`Bastina3D.Application`)

- **Abstrakcije (`IAppDbContext.cs`, `IGeometryFactoryService.cs`)**: Definišu ugovore prema bazi (`Sites`, `Models`, `Media`, `SaveChangesAsync`) i prostornoj fabrici, bez vezivanja aplikacione logike za konkretni ORM ili provajder.
- **SlugGenerator (`SlugGenerator.cs`)**: Pretvara nazive (uključujući naša slova poput `đ` -> `dj` i uklanjanje dijakritika) u URL-friendly slugove ograničene dužine i osigurava jedinstvenost u memoriji (`Unique`) tokom masovnog importa.
- **DTO-ovi (`SiteDtos.cs`)**: Definišu ugovore za čitanje, kreiranje i upite (`SiteListItemDto`, `SiteDetailDto`, `ModelDto`, `MediaDto`, `CreateSiteRequest`, `SiteQuery`) kako se domenski entiteti ne bi direktno izlagali na API-ju.
- **OsmTagMapper (`OsmTagMapper.cs`)**: Mapira OSM tagove u `SiteCategory`, bira glavni i lokalni naziv (`name:en`, `name`, `name:bs` itd.), izvlači općinu i godinu izgradnje (`start_date`) te procjenjuje istorijski period (`Medieval`, `Ottoman`, `Austro-Hungarian`, `Modern`).
- **OsmImporter (`OsmImporter.cs`)**: Preuzima elemente preko `IOverpassClient`, validira WGS84 koordinate i naziv, vrši deduplikaciju po `(OsmType, OsmId)` ili `WikidataId` i radi idempotentan upis/ažuriranje bez prepisivanja postojećih naziva i opisa.
- **WikidataEnricher (`WikidataEnricher.cs`)**: U batch-evima od 50 zapisa dopunjuje lokacije sa `WikidataId` lokalnim nazivom, godinom/periodom, sažetkom sa EN/BS Wikipedije te slikom i metapodacima o autorstvu/licenci (`SiteMedia`) sa Wikimedia Commonsa, uz otpornost na `DbUpdateConcurrencyException`.
- **SiteService (`SiteService.cs`)**: Implementira pretragu i filtriranje po kategoriji, tekstualnom upitu i bounding boxu (`minLon,minLat,maxLon,maxLat`), vraćajući GeoJSON (`FeatureCollection`), paginiranu listu, detalje po `Id`/`Slug` te kreiranje nove lokacije uz provjeru jedinstvenog sluga u bazi.
- **DependencyInjection (`DependencyInjection.cs`)**: Registruje `ISiteService`, `IHeritageImporter` i `IWikidataEnricher` kao `Scoped` servise u DI kontejner.

## Infrastrukturni sloj (`Bastina3D.Infrastructure`)

- **AppDbContext (`AppDbContext.cs`)**: EF Core kontekst (`IAppDbContext`) za `Sites`, `Models` i `Media`, konfiguriše ograničenja dužina kolona, enum konverziju `SiteCategory` u string, indekse (uključujući jedinstveni `Slug` i kompozitni `(OsmType, OsmId)`) te kaskadno brisanje povezanih modela i medija.
- **DbSeeder (`DbSeeder.cs`)**: Inicijalno puni praznu bazu sa nekoliko referentnih spomenika u BiH (Stari most, Latinska ćuprija, Jajačka tvrđava, Počitelj) sa SRID 4326 tačkama.
- **OverpassClient (`OverpassClient.cs`)**: Šalje Overpass QL upit za kulturno i istorijsko naslijeđe na području BiH (`ISO3166-1="BA"`), rotira više javnih Overpass servera uz eksponencijalni backoff kod preopterećenja/grešaka i parsira `lat`/`lon` ili `center` koordinate u `OsmElement` zapise.
- **WikiClient (`WikiClient.cs`)**: Komunicira sa Wikidata (`wbgetentities` za `P18` sliku, `P571` godinu i `en`/`bs` labele/sitelinkove), Wikipedia REST sažecima (`en`/`bs`) i Wikimedia Commons metapodacima (`Artist`, `LicenseShortName`), uz čišćenje HTML-a, obradu nedostupnih stranica i kratke pauze između poziva.
- **DependencyInjection (`DependencyInjection.cs`)**: Registruje `AppDbContext` (sa NetTopologySuite podrškom za SQL Server, uz pripremljenu tačku za Postgres), `IAppDbContext`, typed `HttpClient` instancu za `OverpassClient` (timeout 3 min) i `WikiClient` (timeout 30 s) uz `User-Agent` zaglavlje, te obezbjeđuje `InitializeDatabaseAsync` za automatske migracije i seeding.