# Santa's Workshop Automation - Spec Task List

**Project**: Santa's Workshop Automation  
**Engine**: Unity 2022+ LTS (URP)  
**Target**: PC (Steam)  
**Timeline**: 14-18 months to 1.0  

---

## How to Use This Task List

1. Work through specs in order (dependencies are noted)
2. For each spec, tell Kiro: "Create spec [number]" (e.g., "Create spec 1.1")
3. Check off specs as you complete them
4. Some specs can be done in parallel (noted in groups)

---

## Spec Task List

### **PHASE 1: FOUNDATION** 🏗️

- [x] **1.1: Project Setup & Architecture** (2 weeks | No dependencies)
  - Unity project with URP, folder structure, core architecture, version control

- [x] **1.2: Resource System** (2 weeks | Needs: 1.1)
  - Resource data structures, categories, manager, storage/transfer logic

- [ ] **1.3: Grid & Placement System** (2 weeks | Needs: 1.1)
  - Tile-based grid, placement validation, ghost preview, rotation, snapping

- [ ] **1.4: Machine Framework** (3 weeks | Needs: 1.2, 1.3)
  - Base machine class, states, input/output ports, recipe system, power interface

- [ ] **1.5: Time & Simulation Manager** (1 week | Needs: 1.1)
  - Calendar system, time speed controls, simulation tick, event scheduling

---

**🧪 BUILD & TEST CHECKPOINT #1: Foundation Complete**

Before moving to Phase 2, verify:
- ✅ Project builds without errors in Unity
- ✅ Grid system displays correctly in scene
- ✅ Can place test objects on grid with validation
- ✅ Resources can be created and stored
- ✅ Time controls work (pause, speed up, slow down)
- ✅ Basic machine framework can be instantiated

**Test Scene**: Create a simple test scene with grid, a few test machines, and time controls UI.

---

### **PHASE 2: CORE GAMEPLAY LOOP** 🎮

- [ ] **2.1: Conveyor Belt System** (3 weeks | Needs: 1.2, 1.3, 1.4)
  - Belt machine, item movement, connections, splitters/mergers, performance optimization

- [ ] **2.2: Basic Extractors** (2 weeks | Needs: 1.4, 2.1)
  - Extractor machines, resource nodes, extraction rates, Wood Cutter, Stone Quarry, Iron Mine, Coal Mine

- [ ] **2.3: Basic Processors** (2 weeks | Needs: 1.4, 2.1)
  - Processor machines, refinement recipes, Smelter, Sawmill, Plastic Refinery

- [ ] **2.4: Basic Assemblers** (2 weeks | Needs: 1.4, 2.1, 2.3)
  - Assembler machines, multi-input recipes, General Purpose Assembler, first toy recipes

---

**🧪 BUILD & TEST CHECKPOINT #2: Basic Production Chain**

Before continuing, verify:
- ✅ Can extract raw resources (wood, stone, iron, coal)
- ✅ Resources move along conveyor belts visually and logically
- ✅ Processors refine raw materials (smelter works, sawmill works)
- ✅ Assemblers create first toys (Wooden Train, Teddy Bear)
- ✅ Full chain works: Extractor → Belt → Processor → Belt → Assembler
- ✅ No major performance issues with 20+ machines running

**Test Scene**: Build a small factory that produces one complete toy from raw resources.

---

- [ ] **2.5: Storage & Warehousing** (2 weeks | Needs: 1.2, 1.3)
  - Storage containers (Crate, Depot, Warehouse), capacity tiers, filtering, visual indicators

- [ ] **2.6: Power Grid System** (3 weeks | Needs: 1.4, 1.5)
  - Power grid simulation, generators (Coal, Wind), consumption tracking, connectivity, brownout mechanics

- [ ] **2.7: Packaging & Delivery** (2 weeks | Needs: 1.4, 2.1, 2.4)
  - Wrapping Station, Dispatch Center, gift wrapping, delivery tracking, metrics

- [ ] **2.8: Camera & Input Controls** (1 week | Needs: 1.1, 1.3)
  - Isometric camera, WASD/mouse pan, zoom, rotation, edge scrolling, smooth movement

- [ ] **2.9: Build Menu & Placement UI** (2 weeks | Needs: 1.3, 2.8)
  - Build menu panel, building selection, cost display, hotkeys, placement mode UI

---

**🧪 BUILD & TEST CHECKPOINT #3: Vertical Slice Complete**

This is your first playable build! Verify:
- ✅ Can navigate the world with camera controls smoothly
- ✅ Can open build menu and select buildings
- ✅ Can place buildings with ghost preview and validation
- ✅ Power grid works: generators power machines, brownouts occur when overloaded
- ✅ Storage buildings buffer resources correctly
- ✅ Complete production loop: Extract → Process → Assemble → Wrap → Deliver
- ✅ Delivery tracking shows completed gifts
- ✅ Can play for 10+ minutes without crashes
- ✅ Performance is acceptable (30+ FPS with 50+ buildings)

**Test Scene**: Build a complete mini-factory that delivers at least 10 wrapped gifts.

**Milestone**: This is your M3 Vertical Slice - show this to stakeholders!

---

### **PHASE 3: PROGRESSION & CONTENT** 📈

- [ ] **3.1: Research System** (3 weeks | Needs: 1.5, 2.7)
  - Research tree, 8 branches, Research Points, Research Lab, unlock system, research UI

- [ ] **3.2: Mission System** (3 weeks | Needs: 1.5, 2.7, 3.1)
  - Mission structure, objectives, tracking, completion, rewards, mission UI, tutorial missions

- [ ] **3.3: Quality & Grading System** (2 weeks | Needs: 1.4, 2.4)
  - S/A/B/C quality calculation, grade assignment, Inspection Station, rework loop

---

**🧪 BUILD & TEST CHECKPOINT #4: Progression Systems**

Before adding more content, verify:
- ✅ Research tree displays correctly and is navigable
- ✅ Can complete research and unlock new buildings/recipes
- ✅ Research Lab generates Research Points
- ✅ Mission system tracks objectives and shows completion
- ✅ Tutorial missions guide player through basics
- ✅ Quality grading affects toys (can see S/A/B/C grades)
- ✅ Inspection Station and rework loop function
- ✅ Progression feels rewarding and clear

**Test Scene**: Play through first 3 tutorial missions and unlock 2-3 research nodes.

---

- [ ] **3.4: Expanded Resource Chains** (3 weeks | Needs: 2.2, 2.3, 2.4, 3.1)
  - Tier II-V resources, advanced processors, 20+ toy recipes, component recipes, balancing

- [ ] **3.5: Advanced Logistics** (2 weeks | Needs: 2.1, 3.1)
  - Smart Splitter, Inserters, Underground Belts, Teleport Belts, logistics drones

- [ ] **3.6: Magical Integration System** (3 weeks | Needs: 1.2, 2.6, 3.1)
  - Magical resources, Aurora Harvester, Spirit Conduit, enchanted machines, Aurora Generator

- [ ] **3.7: Seasonal Cycle & Scoring** (2 weeks | Needs: 1.5, 2.7, 3.3)
  - Seasonal progression, Christmas Rush, Spirit Score, year-end summary, carryover system

---

**🧪 BUILD & TEST CHECKPOINT #5: Content & Features Complete**

This is your Alpha build (M4)! Verify:
- ✅ Can produce all Tier I-III toys with expanded resource chains
- ✅ Advanced logistics (Smart Splitter, Underground Belts) work correctly
- ✅ Magical resources can be harvested and used
- ✅ Enchanted machines provide bonuses
- ✅ Aurora Generator provides clean power
- ✅ Seasonal cycle progresses through full year
- ✅ Christmas Rush increases demand and pressure
- ✅ Year-end summary displays correctly with Spirit Score
- ✅ Can carry over progress to next year
- ✅ All major gameplay systems are feature-complete
- ✅ Can play a full year (30-60 minutes) without major issues

**Test Scene**: Play through a complete year, deliver 100+ gifts, achieve B+ Spirit Score.

**Milestone**: This is your M4 Alpha - feature complete!

---

### **PHASE 4: POLISH & INTEGRATION** ✨

- [ ] **4.1: Save/Load System** (3 weeks | Needs: All core systems)
  - Save data schema, serialization, save/load UI, auto-save, multiple slots, versioning

- [ ] **4.2: Inspector & Machine UI** (2 weeks | Needs: 1.4, 2.6, 3.3)
  - Inspector panel, machine status, recipe selection, inventory display, configuration options

- [ ] **4.3: Overlay & Visualization System** (2 weeks | Needs: 2.1, 2.6, 3.6)
  - Overlay framework, Power Grid (F1), Logistics (F2), Quality (F3), Magic (F4) overlays

- [ ] **4.4: Narrative & Dialogue System** (2 weeks | Needs: 3.2)
  - Dialogue system, character definitions, dialogue triggers, storybook UI, story progression

- [ ] **4.5: Audio System & Music** (3 weeks | Needs: 1.1, 1.5)
  - Audio manager, music system (layered, adaptive), SFX, ambient audio, audio settings

- [ ] **4.6: Visual Effects & Polish** (3 weeks | Needs: All machine specs)
  - Particle systems, machine animations, environmental effects, UI animations, visual polish

- [ ] **4.7: Toypedia & Reference UI** (1 week | Needs: 3.4)
  - Toypedia panel, recipe browser, resource/building info, search/filtering, unlock tracking

---

**🧪 BUILD & TEST CHECKPOINT #6: Polish & Integration Complete**

This is your Beta build (M5)! Verify:
- ✅ Can save and load games without data loss
- ✅ Auto-save works reliably
- ✅ Inspector UI shows all machine details clearly
- ✅ All overlay modes (Power, Logistics, Quality, Magic) work
- ✅ Dialogue system displays character interactions
- ✅ Audio and music play correctly, enhance atmosphere
- ✅ Visual effects (particles, animations) polish the experience
- ✅ Toypedia provides useful reference information
- ✅ Game feels cohesive and polished
- ✅ No major UI/UX issues
- ✅ Can play multiple sessions with save/load

**Test Scene**: Play 2-3 full years, save/load between sessions, test all UI features.

**Milestone**: This is your M5 Beta - content complete and polished!

---

### **PHASE 5: OPTIMIZATION & LAUNCH PREP** 🚀

- [ ] **5.1: Performance Optimization** (4 weeks | Needs: All systems)
  - Profiling, Burst + Jobs for conveyors, spatial partitioning, LOD system, GPU instancing

- [ ] **5.2: Balance & Tuning** (3 weeks | Needs: All gameplay systems)
  - Recipe throughput, research pacing, mission difficulty, resource costs, power balance

- [ ] **5.3: Settings & Accessibility** (2 weeks | Needs: 2.8, 4.5)
  - Graphics settings, audio settings, control remapping, accessibility options, language support

- [ ] **5.4: Tutorial & Onboarding** (2 weeks | Needs: 3.2, 4.4)
  - Tutorial polish, contextual tooltips, FTUE, progressive disclosure, help system

- [ ] **5.5: Achievements & Metrics** (1 week | Needs: 3.7, 4.1)
  - Achievement definitions, tracking, Steam API integration, analytics, leaderboards

- [ ] **5.6: Main Menu & Meta UI** (2 weeks | Needs: 4.1)
  - Main menu scene, save selection, new game setup, settings, credits, prestige UI

- [ ] **5.7: Bug Fixing & QA** (4 weeks | Needs: All systems)
  - Bug tracking, crash fixes, edge cases, save corruption prevention, regression testing

---

**🚀 FINAL BUILD & TEST CHECKPOINT #7: Release Candidate**

This is your RC build (M6) - ready for launch! Verify:
- ✅ Consistent 60 FPS with 500+ buildings (or 30 FPS with 1000+)
- ✅ No memory leaks during extended play sessions (2+ hours)
- ✅ All recipes and progression are balanced and fun
- ✅ Tutorial effectively onboards new players
- ✅ Settings and accessibility options work correctly
- ✅ Achievements unlock properly
- ✅ Main menu and meta UI are polished
- ✅ No critical or high-priority bugs remain
- ✅ Game is stable across multiple play sessions
- ✅ All content is accessible and completable
- ✅ Performance targets are met on minimum spec hardware
- ✅ Steam integration works (achievements, cloud saves)

**Test Protocol**:
1. Fresh install test (new player experience)
2. Extended play test (5+ hours in one session)
3. Multiple save files test (10+ saves)
4. Achievement completion test (unlock 50%+ achievements)
5. Performance stress test (build 1000+ buildings)
6. Edge case testing (unusual player behaviors)

**Milestone**: This is your M6 Release Candidate - ship it!

---

## Quick Reference

**Critical Path (Vertical Slice)**: 1.1 → 1.2 → 1.3 → 1.4 → 1.5 → 2.1 → 2.2 → 2.3 → 2.4 → 2.6 → 2.7 → 2.8 → 2.9

**Parallel Development Groups**:
- **Core Systems**: 1.2, 1.4, 2.6
- **Logistics**: 1.3, 2.1, 3.5
- **Content**: 2.2, 2.3, 2.4, 3.4
- **UI/UX**: 2.8, 2.9, 4.2, 4.3
- **Progression**: 3.1, 3.2, 3.7
- **Polish**: 4.4, 4.5, 4.6

**Total Specs**: 32  
**Estimated Duration**: 14-18 months  
**Team Size**: 8-12 developers

---

**Document Version**: 2.0 (Task List Format)  
**Last Updated**: November 2, 2025  
**Status**: Ready for Spec Creation
