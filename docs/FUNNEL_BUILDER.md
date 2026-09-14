# Funnel Builder – MVP

## Umfang

- Admin unter `/tools/funnels`, Editor unter `/tools/funnels/{id}`. Nutzt MainLayout, Bootstrap, EditForm, CurrentTenantService und ModuleAccessService.
- Freigabe `funnel-builder` sowohl beim Mandanten als auch beim Benutzer aktivieren. Alle Admin-Lese-/Schreibzugriffe prüfen Modulfreigabe und aktuellen Mandanten erneut.
- Metadaten, eindeutiger globaler Slug, Entwurf/Veröffentlichung, bis zu 30 geordnete Schritte: Text, Einzelauswahlfrage, Kontaktformular und Reveal. Reveal zeigt den Inhalt beim Erreichen des Schrittes.
- Öffentliche Blazor-Runtime unter `/funnel/{slug}` bzw. `/funnel` auf einer zugeordneten Domain, mit bestehendem PublicLayout. Im MVP logisch getrennt, noch kein separater Prozess/Deployment.
- Inhalte werden als Text ausgegeben, nicht als frei ausführbares HTML. Platzhalter: `{{Vorname}}`, `{{Nachname}}`, `{{Firma}}`, `{{EmpfaengerId}}` sowie `{{Empfänger-ID}}`.
- Erster Aufruf ohne `id` erstellt Recipient + Lead. Optionale URL-Werte: `vorname`, `nachname` (alternativ `name`), `firma`. Danach ersetzt die Runtime die URL durch `?id=<zufällige UUID>`; Personenparameter werden entfernt. Diese UUID ist ein vertraulicher Zugriffslink, keine laufende Kundennummer. Eine unbekannte/ungültige UUID legt keinen neuen Empfänger an. Ein vorhandener Link überschreibt dessen Stammdaten nicht durch URL-Parameter.
- `open`, `step`, `reveal`, `questions`, `complete` werden als einmalige Meilensteine gespeichert. `LastSeenAt` wird zusätzlich aktualisiert. Antworten (nach Schritt-ID) und Kontaktdaten landen in genau einem FunnelLead pro Recipient. Abgeschlossene Links zeigen die Bestätigung, statt erneut abzuschließen.
- Unvollständige Besuche starten bei erneutem Laden im MVP beim ersten Schritt; gespeicherte Antworten bleiben erhalten. Kontaktformular verlangt Vorname, Nachname und gültige E-Mail; Telefon ist optional.
- `VideoBinding` hält PVS-Public-ID und Variablen-JSON pro Schritt bereit. Noch keine PVS-Aufrufe, Videowiedergabe oder Zugangsdaten.

## Einrichtung

1. Migration `20260913142217_AddFunnelBuilder` mit dem vorhandenen Deployment-Verfahren anwenden. Der bestehende App-Start führt Migrationen ohnehin automatisch aus. Die Implementierung wurde nicht gegen eine produktive Datenbank gestartet.
2. Modul für Mandant und Benutzer in der vorhandenen Administration aktivieren.
3. Technische Hosts explizit konfigurieren, z. B. `Funnels__RuntimeHosts__0=funnels.example.com`. Die Standardliste ist leer; Development erlaubt `localhost` und `127.0.0.1`. Nur auf diesen Hosts erfolgt die Auflösung per Slug.
4. Funnel mit mindestens einem Schritt anlegen und veröffentlichen. Lokal z. B. `/funnel/pilot?vorname=Ada&nachname=Lovelace&firma=Beispiel` aufrufen.
5. Eigene Domain im Editor vormerken. Nach manueller Eigentums-/DNS-Prüfung setzt der Betreiber den zugehörigen `DomainMappings.IsEnabled`-Wert auf `true`. Im MVP gibt es dafür keine zusätzliche Verwaltungsoberfläche. Exakter Hostname, ohne Schema, Port oder Pfad; www und nicht-www sind eigenständige Einträge. Ein aktives Mapping hat Vorrang vor einem URL-Slug. Der Root-Aufruf einer aktiven Domain leitet nach `/funnel` weiter und erhält die Queryparameter.
6. DNS, Reverse Proxy und TLS separat einrichten. Der Proxy muss den ursprünglichen Host korrekt weiterreichen und eingehende Forwarded-Header bereinigen. Das Repository vertraut bereits global Forwarded-Headern; die bestehende Konfiguration wurde nicht erweitert. Der interne App-Port sollte nur über den vertrauenswürdigen Proxy erreichbar sein.

Deaktivierte Mandanten, deaktivierte Modulfreigaben, unveröffentlichte Funnels und unbekannte Hosts werden von der Runtime abgewiesen. Laufende Formulare verlangen nach einer Änderung/Deaktivierung des Funnels einen Reload. Gleichzeitige Empfängeränderungen werden durch eine EF-Concurrency-Prüfung vor stillem Überschreiben geschützt.

## Dateien

- `Models/Funnel.cs`: sieben Entities und Step-Typen; `Lead` ist bewusst die Funnel-Antwortstruktur. Kein automatischer Export in den bestehenden Lead Optimizer.
- `Data/ApplicationDbContext.cs`: Sets, Beziehungen, Indizes und Concurrency-Prüfung.
- `Data/ApplicationDbContextFactory.cs`: sichere Design-Time-Erstellung mit derselben Identity-Schemaversion wie die App; keine Startup-Worker oder Datenbankmigrationen. Für explizite EF-Datenbankbefehle `ConnectionStrings__DefaultConnection` setzen; Standard ist eine lokale Design-Time-Verbindung.
- `Migrations/20260913142217_AddFunnelBuilder.cs`, zugehörige Designer-Datei und `ApplicationDbContextModelSnapshot.cs`: ausschließlich additive Funnel-Tabellen und Beziehungen.
- `Services/FunnelAdminService.cs`, `Services/FunnelRuntimeService.cs`: Speichern, Berechtigungen, Hostauflösung, Personalisierung und Tracking.
- `Components/Pages/Tools/FunnelBuilder.razor`, `Components/Pages/Public/FunnelRuntime.razor`: Übersicht/Editor und öffentliche Seiten.
- `Components/Layout/MainLayout.razor`, `Components/Pages/AdminUsers.razor`, `Components/Pages/Admin/TenantModules.razor`: Navigation und Freigabe.
- `Program.cs`: Dienstregistrierungen und Domain-Root-Weiterleitung.
- `appsettings.json`, `appsettings.Development.json`: explizite Runtime-Hosts.
- `ToolboxPortal.csproj`, `tests/FunnelSmoke/*`: dependencyfreier Test-Runner mit vorhandenen Paketen; Testquellen/-ausgaben werden aus der Web-App ausgeschlossen.

## Prüfung

Ergebnis: Build erfolgreich (0 Fehler, 4 bestehende Warnungen), alle 32 Smoke-Prüfungen bestanden, keine ausstehenden Modelländerungen. PostgreSQL-Migrationsskript erfolgreich erzeugt und auf ausschließlich additive Änderungen geprüft.

```sh
dotnet build
dotnet run --project tests/FunnelSmoke
dotnet ef migrations has-pending-model-changes
dotnet ef migrations script 20260520210328_AddTenantModuleAccesses 20260913142217_AddFunnelBuilder
```

Der Smoke-Test verwendet ausschließlich eine flüchtige SQLite-Datenbank und startet nicht die normale App. Er prüft Mandantentrennung, Modulrechte, Slug-Konflikte, Domain-Normalisierung/-Freischaltung, unbekannte Hosts, Empfänger-Token, Personalisierung, Antwort-/Kontaktvalidierung, vollständigen Durchlauf, zusammengeführte Leads, idempotente Meilensteine, Schritt-Reihenfolge und Historie nach dem Entfernen von Schritten.

Grenze: SQLite-Funktionstests ersetzen keinen PostgreSQL-Deploymenttest. Ein Browser-/Proxy-/TLS-Test auf einer Staging-Instanz steht noch aus. Vorhandene Buildwarnungen zu SQLitePCLRaw, veralteter ForwardedHeaders-API und zwei Nullable-Stellen im THG-Import wurden nicht durch Paket-/Fremdmoduländerungen ausgeweitet.

## Nächster kleiner Schritt

Den ersten Pilot-Funnel samt Empfängerliste auf Staging mit PostgreSQL und einer Testdomain prüfen. CSV-Import, Datenschutz-/Impressumsbausteine für den konkreten öffentlichen Einsatz, PVS-Auslieferung, Funnel-Versionierung und DNS-/SSL-Automation sind weitere Ausbaustufen.

## Erweiterung: Empfänger und Tracking

Unter `/tools/funnels/{id}/recipients` gibt es jetzt eine mandantengeschützte Empfängerliste (50 Einträge pro Seite), manuelle Empfängeranlage, auswählbare persönliche Links sowie Kontakt-/Antwortdetails und den chronologischen Ereignisverlauf. Einstieg über Übersicht oder Editor. „Aktualisieren“ lädt neue Ergebnisse; Zeitangaben sind explizit UTC. Es werden keine Einladungen verschickt.

Vorbereitete Empfänger haben den Status `pending` und leere Aufrufzeitstempel. Erst das Öffnen ihres UUID-Links setzt den ersten Aufruf; dabei wird derselbe Datensatz weiterverwendet. Die zusätzliche Migration `PrepareFunnelRecipients` macht deshalb `OpenedAt` und `LastSeenAt` nullable; vorhandene Besuchszeiten bleiben erhalten.

Links bevorzugen eine aktive Kampagnendomain. Als Alternative `Funnels__PublicOrigin=https://funnels.example.com` konfigurieren; der Host muss auch in `Funnels:RuntimeHosts` stehen. Lokal kann die Adresse inklusive Port verwendet werden, z. B. `http://localhost:5000`. Ohne passende Konfiguration zeigt die Oberfläche einen Hinweis statt eines falschen Links. Die Linkfelder lassen sich per Klick vollständig markieren und kopieren. Entwürfe werden deutlich als noch nicht nutzbar gekennzeichnet.

Zusätzlich geändert: `Models/Funnel.cs`, beide Funnel-Services, `FunnelBuilder.razor`, neue `FunnelRecipients.razor`, Migration samt Designer/Snapshot und der Smoke-Test. Keine neuen Pakete. Die zusätzlichen Tests prüfen vorab angelegte Empfänger, echte Erstaufrufe, Linkauswahl, Mandantengrenzen und Seitennavigation. Browser- und PostgreSQL-Stagingtest bleiben offen; noch kein Push oder Deployment.
