# Dragon Nest / BDO Master Quest Chains

## [TUT_01] Resolve
- Category: MainStory
- Scene: Tutorial
- Prerequisite: 
- StartNPC: Warrior | TurnInNPC: Warrior | NextQuest: TUT_02
- Objective: RequirementQuest | Target: Dummy | Amount: 10 | GoldReward: 5
- Description: Attack the Training Dummy to prove your resolve to the Warrior Chief.
- OfferDialogue:
  Warrior: Hello, traveler. I understand you want to help, but I cannot send you to your death.
  Warrior: Show me your resolve! Attack the Training Dummy until you deal 10 damage.
  Player: Watch me strike.
- InProgressDialogue:
  Warrior: Keep striking the Training Dummy. Show me your true technique!
- TurnInDialogue:
  Warrior: Impressive strikes! I can see your potential, but be careful out there.
  Warrior: Take my insignia and show it to the Elf gatekeeper. She will allow you to pass.

## [TUT_02] Explore the Dense Forest
- Category: MainStory
- Scene: Tutorial
- Prerequisite: TUT_01
- StartNPC: Warrior | TurnInNPC: Elf | NextQuest: SC1_01
- Objective: TalkToNPC | Target: Elf | Amount: 1 | GoldReward: 5
- Description: Present the Warrior's Insignia to the Elf Gatekeeper to unlock the forest pathway.
- OfferDialogue:
  Warrior: Head over to the eastern gate. The Elf guards the entrance to the forest.
  Player: I'll speak with her at once.
- InProgressDialogue:
  Elf: STOP RIGHT THERE! Who are you? You cannot enter the forest without the Knight's approval!
- TurnInDialogue:
  Elf: Ah, I see the insignia of the Knight Chief in your hand.
  Elf: Very well, traveler. I'll remove the barrier and grant you permission to pass into the forest.

## [SC1_01] Lingering Corruption
- Category: MainStory
- Scene: Scene1
- Prerequisite: TUT_02
- StartNPC: Red Knight | TurnInNPC: Red Knight | NextQuest: SC2_01
- Objective: EnemyExtermination | Target: Blue Slime | Amount: 4 | GoldReward: 10
- Description: Defeat 4 corrupted Blue Slimes to defend the forest outpost.
- OfferDialogue:
  Red Knight: Are you the traveler who was acknowledged by my apprentice? Good!
  Red Knight: The influence of the Demon King is corrupting the elemental spirits into foul slimes.
  Red Knight: Slay 4 Blue Slimes before they escape the perimeter and threaten nearby villages!
  Player: Consider it done.
- InProgressDialogue:
  Red Knight: The slimes are still multiplying. Eliminate 4 of them to secure the area!
- TurnInDialogue:
  Red Knight: Thank you, traveler, for helping me defend the outpost!
  Red Knight: Be cautious ahead—the creatures of the deep forest grow far more aggressive.

## [SC2_01] Dire Situation
- Category: MainStory
- Scene: Scene2
- Prerequisite: SC1_01
- StartNPC: Merchant | TurnInNPC: Merchant | NextQuest: SC2_02
- Objective: EnemyExtermination | Target: HellBeast | Amount: 6 | GoldReward: 15
- Description: Slay 6 demonic beasts threatening the merchant caravan.
- OfferDialogue:
  Merchant: Argh! Traveler, please help us! Demonic beasts ambushed my caravan!
  Merchant: Drive back 6 of these monsters so we can search the area!
  Player: Hold tight, I'll clear the monsters!
- InProgressDialogue:
  Merchant: The beasts are still prowling close by. Please defeat 6 of them!
- TurnInDialogue:
  Merchant: You defeated them! But my daughter ran deeper into the woods to escape...
  Merchant: Please, you must find her!

## [SC2_02] Dire Situation II
- Category: MainStory
- Scene: Scene2
- Prerequisite: SC2_01
- StartNPC: Merchant | TurnInNPC: Daughter | NextQuest: SC3_01
- Objective: TalkToNPC | Target: Daughter | Amount: 1 | GoldReward: 10
- Description: Locate the Merchant's lost daughter in the forest clearing.
- OfferDialogue:
  Merchant: She was fleeing toward the northern path. Bring her to safety!
  Player: I'll bring her back safely.
- InProgressDialogue:
  Daughter: Where is my father? Have you seen him? Please help me!
- TurnInDialogue:
  Daughter: You spoke with my father? Oh, thank heavens!
  Daughter: Thank you so much, brave traveler! The barrier to the grove is open now!

## [SC3_01] Wraith
- Category: MainStory
- Scene: Scene3
- Prerequisite: SC2_02
- StartNPC: Elf Scout | TurnInNPC: Elf Scout | NextQuest: SC3_02
- Objective: EnemyExtermination | Target: Wraith | Amount: 3 | GoldReward: 20
- Description: Eliminate 3 Wraiths to rescue the pinned Hero.
- OfferDialogue:
  Elf Scout: Argh!... Who are you? The Knight's Insignia?! You came to help?
  Elf Scout: You must help the Hero! He sacrificed himself to draw the resentment spirits away from me!
  Elf Scout: Slay 3 Wraiths surrounding the inner sanctuary!
  Player: I'm on it!
- InProgressDialogue:
  Elf Scout: The demonic energy calls forth resentful spirits. Slay the 3 Wraiths!
- TurnInDialogue:
  Elf Scout: You banished the wraiths! Quick, go speak with the Hero before he confronts the demon lord!

## [SC3_02] Astaroth
- Category: MainStory
- Scene: Scene3
- Prerequisite: SC3_01
- StartNPC: Hero | TurnInNPC: Hero | NextQuest: SC4_01
- Objective: ScoreChecker | Target: Gold | Amount: 10 | GoldReward: 25
- Description: Gather 10 Gold (Shards of Urim) to empower the blade for the final battle against Astaroth.
- OfferDialogue:
  Hero: A traveler? Why are you here so deep in the forest?
  Hero: What?! The outpost knights acknowledged you? Then we truly have a fighting chance.
  Hero: Beyond this point lies Astaroth, the Demon King. To pierce his dark aegis, you must possess 10 Shards of Urim (Gold).
  Player: I will find the shards and face the Demon King.
- InProgressDialogue:
  Hero: Search the ancient chests and defeat remaining foes. You need 10 Gold (Shards of Urim) before entering the portal!
- TurnInDialogue:
  Hero: You have collected all 10 Shards of Urim?!
  Hero: Extraordinary! With that power in your hand, Astaroth can be slain once and for all. Enter the portal!

## [SC4_01] The Legend of Lam-ang
- Category: MainStory
- Scene: Scene4
- Prerequisite: SC3_02
- StartNPC: Princess | TurnInNPC: Princess | NextQuest: 
- Objective: TalkToNPC | Target: Princess | Amount: 1 | GoldReward: 50
- Description: Hear the ancient tale of heroism before vanquishing the Demon King.
- OfferDialogue:
  Princess: You know, someone like you saved our land long ago. His name was Lam-ang.
  Princess: Strong as anyone, even as a child. He set out to avenge his fallen kin with courage unmatched.
  Princess: Accompanied by a fierce gray forest dog, he conquered the darkness and returned light to our realm.
  Princess: Now it is your turn, hero. Cleanse this throne room!
  Player: The Demon King falls today.
- InProgressDialogue:
  Princess: Slay the Demon King in the throne room and restore our kingdom!
- TurnInDialogue:
  Princess: You have triumphed over the Demon King! Peace is restored across the realm!

