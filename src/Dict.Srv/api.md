# Web API Specification

Operation | Verb | Url | Return
----------|------|-----|-------
**Collections** |
Get List | GET | /api/collections | item array
Get Item | GET | /api/collections/{id} | item
Add | POST | /api/collections | item
Update | PUT | /api/collections/{id}
Delete | Delete | /api/collections/{id}
**Cards** |
Get List | GET | /api/cards/collection/{collectionId} | list[?first=f&count=n]
Get Item | GET | /api/cards/{cardId} | item
Add | POST | /api/cards | item
Update | PUT | /api/cards/{collectionId} | item
Delete | DELETE | /api/cards/{cardId} | item
