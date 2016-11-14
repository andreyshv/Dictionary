BEGIN TRANSACTION;
CREATE TABLE "Settings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Settings" PRIMARY KEY AUTOINCREMENT,
    "SizeSet" INTEGER NOT NULL
);
CREATE TABLE "Repetitions" (
    "CardId" INTEGER NOT NULL CONSTRAINT "PK_Repetitions" PRIMARY KEY AUTOINCREMENT,
    "EasynessFactior" REAL NOT NULL,
    "Interval" REAL NOT NULL,
    "Iteration" INTEGER NOT NULL,
    "NextRepetition" TEXT NOT NULL
);
CREATE TABLE "Medias" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Medias" PRIMARY KEY AUTOINCREMENT,
    "Hash" TEXT,
    "MediaType" INTEGER NOT NULL,
    "Name" TEXT,
    "Source" TEXT
);
CREATE TABLE "Collections" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Collections" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT,
    "Name" TEXT NOT NULL
);
CREATE TABLE "Cards" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Cards" PRIMARY KEY AUTOINCREMENT,
    "CollectionId" INTEGER NOT NULL,
    "Context" TEXT,
    "ImageId" INTEGER NOT NULL,
    "SoundId" INTEGER NOT NULL,
    "Transcription" TEXT,
    "Translation" TEXT,
    "Word" TEXT,
    CONSTRAINT "FK_Cards_Collections_CollectionId" FOREIGN KEY ("CollectionId") REFERENCES "Collections" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Cards_Medias_ImageId" FOREIGN KEY ("ImageId") REFERENCES "Medias" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Cards_Medias_SoundId" FOREIGN KEY ("SoundId") REFERENCES "Medias" ("Id") ON DELETE CASCADE
);

INSERT INTO `Medias` VALUES 
 (1,'1EE503C96C783BEF5855FAE4DD0A00DC',1,'1.png',''),
 (2,'57C6052B50E6E29F143489786496433F',1,'2.png',''),
 (3,'3EDAF10A0ABA2F0C9F07CCB0E766F354',1,'3.png',''),
 (4,'4F5ABC1846801CB3BA63DF9CA801D0F4',1,'4.png',''),
 (5,'1E706C6BDF31F888E5F1369FFA074386',1,'5.png',''),
 (6,'7EC4AE85F431973EA481DB67E1015F8F',2,'6.mp3',''),
 (7,'3C36AFAA6D9B9A58494E3C973F8D6DDD',2,'7.mp3',''),
 (8,'07CA9EF58E2FC50DDC5071B3292EC248',2,'8.mp3',''),
 (9,'39ABDAD0D743DC7A54650E9CBFE8DB23',2,'9.mp3',''),
 (10,'E48ECF7214D44A214432F4AE6B4CB346',2,'10.mp3','');

INSERT INTO `Collections` VALUES (1,'Seed','TestCollection');

INSERT INTO `Cards` VALUES 
 (1,1,'',1,6,'ətˈenjʊɛɪːt','ослаблять','attenuate'),
 (2,1,'',2,7,'ɪntəvˈiːnɪŋ','промежуточный','intervening'),
 (3,1,'',3,8,'ɛnˈgeɪʤ','участвовать','engage'),
 (4,1,'',4,9,'hˈeɪːzi','туманный','hazy'),
 (5,1,'',5,10,'hˈɜːdl','препятствие','hurdle'),
 (6,1,'',1,6,'ɪntˈenʃənəli','преднамеренно','intentionally'),
 (7,1,'',2,7,'nˈʌdʒ','лёгкий толчок локтем','nudge'),
 (8,1,'',3,8,'peɪs','темп','pace'),
 (9,1,'',4,9,'flˈeʃ ˈaʊːt','конкретизировать','flesh out'),
 (10,1,'',5,10,'gˈet ə grˈɪp ɒn','получить контроль над','get a grip on'),
 (11,1,'',1,6,'əbrˈʌptli','внезапно','abruptly'),
 (12,1,'',2,7,'kouzi','уютный','cozy'),
 (13,1,'',3,8,'weɪl','кит','whale'),
 (14,1,'',4,9,'vənˈækjʊlə','родной язык','vernacular'),
 (15,1,'',5,10,'rˈɪg ʌp','строить наспех','rig up'),
 (16,1,'',1,6,'tˈeɪːm','приручать','tame'),
 (17,1,'',2,7,'bi j̆ɒn ðə lˈʊkaʊːt','быть на чеку','be on the lookout'),
 (18,1,'',3,8,'hˈʌmp','горб','hump'),
 (19,1,'',4,9,'ɪnˊtestɪn','кишечник','intestine'),
 (20,1,'',5,10,'ˈaʊːt lˈaʊːd','Вслух','out loud');
 
CREATE INDEX "IX_Cards_SoundId" ON "Cards" ("SoundId");
CREATE INDEX "IX_Cards_ImageId" ON "Cards" ("ImageId");
CREATE INDEX "IX_Cards_CollectionId" ON "Cards" ("CollectionId");
COMMIT;
