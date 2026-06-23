// ============================================================
// Slot Rogue Battle - Web Prototype
// ============================================================

// --- Skill Database ---
const SKILLS = {
    strike:       { id:'strike',       name:'ストライク',     effect:'deal_damage',      value:6,  desc:'6ダメージを与える',        rarity:'common',   icon:'⚔️' },
    heavy_strike: { id:'heavy_strike', name:'ヘビーストライク', effect:'deal_damage',     value:10, desc:'10ダメージを与える',       rarity:'uncommon', icon:'🗡️' },
    guard:        { id:'guard',        name:'ガード',         effect:'gain_block',       value:5,  desc:'ブロック5を得る',           rarity:'common',   icon:'🛡️' },
    iron_wall:    { id:'iron_wall',    name:'アイアンウォール', effect:'gain_block',      value:8,  desc:'ブロック8を得る',           rarity:'uncommon', icon:'🏰' },
    heal:         { id:'heal',         name:'ヒール',         effect:'heal',             value:4,  desc:'HP4回復',                  rarity:'uncommon', icon:'💚' },
    poison_blade: { id:'poison_blade', name:'毒刃',           effect:'apply_poison',     value:3,  desc:'毒3を付与',                rarity:'uncommon', icon:'🧪' },
    expose:       { id:'expose',       name:'弱体化',         effect:'apply_vulnerable', value:2,  desc:'脆弱2を付与',              rarity:'uncommon', icon:'💥' },
    power_up:     { id:'power_up',     name:'パワーアップ',    effect:'apply_strength',  value:2,  desc:'筋力+2',                   rarity:'rare',     icon:'💪' },
    draw_spin:    { id:'draw_spin',    name:'加速',           effect:'draw_spin',        value:1,  desc:'スピン+1',                 rarity:'rare',     icon:'🔄' },
    blood_strike: { id:'blood_strike', name:'血の一撃',       effect:'self_damage',      value:14, desc:'自傷4 → 14ダメージ',       rarity:'rare',     icon:'🩸' },
    reckless:     { id:'reckless',     name:'捨て身',         effect:'self_damage',      value:8,  desc:'自傷2 → 8ダメージ',        rarity:'common',   icon:'💢' },
    double_edge:  { id:'double_edge',  name:'諸刃の剣',      effect:'self_damage',      value:20, desc:'自傷6 → 20ダメージ',       rarity:'rare',     icon:'⚡' },
};

const SKILL_POOL = Object.values(SKILLS);

// --- Enemy Database ---
const ENEMIES = {
    slime:     { name:'スライム',     hp:40,  sprite:'🟢', intents:[{type:'attack',value:8},{type:'defend',value:5},{type:'attack',value:10}] },
    goblin:    { name:'ゴブリン',     hp:35,  sprite:'👺', intents:[{type:'attack',value:6},{type:'attack',value:6},{type:'buff',value:2}] },
    skeleton:  { name:'スケルトン',   hp:50,  sprite:'💀', intents:[{type:'attack',value:12},{type:'defend',value:8},{type:'debuff',value:2}] },
    orc:       { name:'オーク',       hp:65,  sprite:'👹', intents:[{type:'attack',value:10},{type:'buff',value:3},{type:'attack',value:14}] },
    dragon:    { name:'ドラゴン',     hp:100, sprite:'🐉', intents:[{type:'attack',value:15},{type:'defend',value:12},{type:'attack',value:20},{type:'buff',value:3}] },
};

const FLOOR_ENEMIES = {
    battle: ['slime','goblin'],
    elite:  ['skeleton','orc'],
    boss:   ['dragon'],
};

// --- Characters ---
const CHARACTERS = {
    striker: {
        name:'ストライカー', hp:80, spins:2, mechanic:'aim',
        reels: [
            [SKILLS.strike, SKILLS.strike, SKILLS.heavy_strike, SKILLS.guard],
            [SKILLS.strike, SKILLS.guard, SKILLS.strike, SKILLS.guard],
            [SKILLS.guard, SKILLS.guard, SKILLS.iron_wall, SKILLS.strike],
        ]
    },
    berserker: {
        name:'バーサーカー', hp:70, spins:2, mechanic:'berserk',
        reels: [
            [SKILLS.reckless, SKILLS.strike, SKILLS.blood_strike, SKILLS.strike],
            [SKILLS.strike, SKILLS.reckless, SKILLS.guard, SKILLS.strike],
            [SKILLS.guard, SKILLS.strike, SKILLS.guard, SKILLS.reckless],
        ]
    }
};

// --- Shop Items ---
const RELIC_SHOP = [
    { name:'スピン+1', desc:'毎ターンスピン+1', cost:120, effect:'extra_spin' },
    { name:'開幕ブロック', desc:'戦闘開始時ブロック5', cost:100, effect:'starting_block', value:5 },
];

// ============================================================
// Game State
// ============================================================
class Game {
    constructor() {
        this.screens = {};
        document.querySelectorAll('.screen').forEach(s => {
            this.screens[s.id.replace('screen-','')] = s;
        });
    }

    // --- Screen Management ---
    showScreen(name) {
        Object.values(this.screens).forEach(s => s.classList.remove('active'));
        const screen = this.screens[name];
        if (screen) {
            screen.classList.add('active');
            screen.style.display = 'flex';
        }
    }

    // --- Title ---
    showCharacterSelect() { this.showScreen('charselect'); }

    // --- Character Select ---
    selectCharacter(id) {
        const ch = CHARACTERS[id];
        this.run = {
            characterId: id,
            characterName: ch.name,
            mechanic: ch.mechanic,
            maxHP: ch.hp,
            currentHP: ch.hp,
            baseSpins: ch.spins,
            gold: 100,
            floor: 0,
            reels: ch.reels.map(r => [...r]),
            relics: [],
            map: null,
            visitedNodes: new Set(),
            currentNodeId: null,
        };
        // Berserk tracker
        this.berserkSelfDamage = 0;
        // Aim gauge
        this.aimFill = 0;
        this.aimActive = false;
        this.overclockBonus = 0;

        this.generateMap();
        this.showMap();
    }

    // --- Map ---
    generateMap() {
        const floors = 15;
        let id = 0;
        const nodes = [];
        const floorNodes = [];

        for (let f = 0; f < floors; f++) {
            const type = this.getFloorType(f, floors);
            let width = (f === 0 || f === floors - 1) ? 1 : 2 + Math.floor(Math.random() * 2);
            if (type === 'boss') width = 1;

            const currentFloor = [];
            for (let c = 0; c < width; c++) {
                let nodeType = type;
                if (type === 'battle' && Math.random() < 0.15) nodeType = 'event';
                const node = { id: id++, floor: f, col: c, type: nodeType, connections: [], visited: false, accessible: false };
                currentFloor.push(node);
                nodes.push(node);
            }

            if (f > 0) {
                const prev = floorNodes[f - 1];
                // Connect
                prev.forEach(pn => {
                    const closest = Math.min(pn.col, currentFloor.length - 1);
                    pn.connections.push(currentFloor[closest].id);
                    if (closest + 1 < currentFloor.length && Math.random() < 0.4)
                        pn.connections.push(currentFloor[closest + 1].id);
                    if (closest - 1 >= 0 && Math.random() < 0.4)
                        pn.connections.push(currentFloor[closest - 1].id);
                    pn.connections = [...new Set(pn.connections)];
                });
                // Ensure all current nodes reachable
                currentFloor.forEach(cn => {
                    const hasIncoming = prev.some(pn => pn.connections.includes(cn.id));
                    if (!hasIncoming) {
                        const rp = prev[Math.floor(Math.random() * prev.length)];
                        rp.connections.push(cn.id);
                    }
                });
            }

            floorNodes.push(currentFloor);
        }

        // First floor accessible
        floorNodes[0].forEach(n => n.accessible = true);
        this.run.map = nodes;
    }

    getFloorType(f, total) {
        if (f === total - 1) return 'boss';
        if (f === 0) return 'battle';
        if (f === 4 || f === 9) return 'shop';
        if (f === 6 || f === 12) return 'rest';
        if (f === 7 || f === 11) return 'elite';
        return 'battle';
    }

    showMap() {
        this.showScreen('map');
        document.getElementById('map-hp').textContent = `HP: ${this.run.currentHP}/${this.run.maxHP}`;
        document.getElementById('map-gold').textContent = `Gold: ${this.run.gold}`;
        document.getElementById('map-floor').textContent = `Floor: ${this.run.floor + 1}`;

        const container = document.getElementById('map-container');
        container.innerHTML = '';

        // Group by floor
        const floors = {};
        this.run.map.forEach(n => {
            if (!floors[n.floor]) floors[n.floor] = [];
            floors[n.floor].push(n);
        });

        const maxFloor = Math.max(...Object.keys(floors).map(Number));
        for (let f = 0; f <= maxFloor; f++) {
            const floorDiv = document.createElement('div');
            floorDiv.className = 'map-floor';
            const label = document.createElement('div');
            label.className = 'map-floor-label';
            label.textContent = f + 1;
            floorDiv.appendChild(label);

            (floors[f] || []).forEach(node => {
                const nodeDiv = document.createElement('div');
                const icons = { battle:'⚔️', elite:'💀', shop:'🛒', event:'❓', rest:'🏕️', boss:'👑' };
                const labels = { battle:'Battle', elite:'Elite', shop:'Shop', event:'Event', rest:'Rest', boss:'BOSS' };
                nodeDiv.className = `map-node node-${node.type}`;
                if (node.visited) nodeDiv.classList.add('visited');
                else if (node.accessible) nodeDiv.classList.add('accessible');
                else nodeDiv.classList.add('disabled');

                nodeDiv.innerHTML = `<div class="node-icon">${icons[node.type]||'?'}</div><div class="node-label">${labels[node.type]||'?'}</div>`;

                if (node.accessible && !node.visited) {
                    nodeDiv.onclick = () => this.selectNode(node);
                }

                floorDiv.appendChild(nodeDiv);
            });

            container.appendChild(floorDiv);
        }
    }

    selectNode(node) {
        node.visited = true;
        this.run.visitedNodes.add(node.id);
        this.run.currentNodeId = node.id;
        this.run.floor = node.floor;

        // Unlock connected nodes
        node.connections.forEach(cid => {
            const cn = this.run.map.find(n => n.id === cid);
            if (cn) cn.accessible = true;
        });

        switch (node.type) {
            case 'battle': case 'elite': case 'boss':
                this.startBattle(node.type);
                break;
            case 'shop':
                this.showShop();
                break;
            case 'rest':
                this.showRest();
                break;
            case 'event':
                // Simple event: random gold or heal
                if (Math.random() < 0.5) {
                    const g = 15 + Math.floor(Math.random() * 20);
                    this.run.gold += g;
                    alert(`宝箱を見つけた！ +${g} Gold`);
                } else {
                    const h = 5 + Math.floor(Math.random() * 10);
                    this.run.currentHP = Math.min(this.run.maxHP, this.run.currentHP + h);
                    alert(`泉を見つけた！ HP ${h} 回復`);
                }
                this.showMap();
                break;
        }
    }

    // --- Battle ---
    startBattle(type) {
        this.showScreen('battle');

        // Pick enemy
        const pool = FLOOR_ENEMIES[type] || FLOOR_ENEMIES.battle;
        const enemyId = pool[Math.floor(Math.random() * pool.length)];
        const enemyData = ENEMIES[enemyId];

        this.battle = {
            player: { hp: this.run.currentHP, maxHP: this.run.maxHP, block: 0, statuses: {} },
            enemy: { hp: enemyData.hp, maxHP: enemyData.hp, block: 0, statuses: {}, name: enemyData.name, sprite: enemyData.sprite },
            intents: enemyData.intents,
            intentIndex: 0,
            turn: 0,
            spins: 0,
            spinning: false,
            reelsStopped: [false, false, false],
            reelResults: [null, null, null],
            reelTimers: [0, 0, 0],
            animFrame: null,
            lastTime: 0,
        };

        // Reset per-battle state
        this.berserkSelfDamage = 0;
        this.aimFill = 0;
        this.aimActive = false;

        // Init UI
        document.getElementById('enemy-name').textContent = enemyData.name;
        document.getElementById('enemy-sprite').textContent = enemyData.sprite;
        document.getElementById('battle-log').innerHTML = '';
        this.updateBattleUI();
        this.setupMechanicUI();
        this.startPlayerTurn();
    }

    setupMechanicUI() {
        const area = document.getElementById('mechanic-area');
        if (this.run.mechanic === 'aim') {
            area.innerHTML = `
                <div class="aim-gauge-container">
                    <span>目押し:</span>
                    <div class="aim-gauge-bar"><div class="aim-gauge-fill" id="aim-fill" style="width:0%"></div></div>
                    <button class="btn btn-aim" id="aim-btn" onclick="game.activateAim()" disabled>AIM</button>
                    <button class="btn btn-overclock" onclick="game.activateOverclock()">OC</button>
                </div>`;
        } else if (this.run.mechanic === 'berserk') {
            area.innerHTML = `<div class="berserk-info" id="berserk-info">バーサーク倍率: x1.00</div>`;
        } else {
            area.innerHTML = '';
        }
    }

    startPlayerTurn() {
        const b = this.battle;
        b.turn++;
        b.player.block = 0;

        // Poison tick
        if (b.player.statuses.poison > 0) {
            const dmg = b.player.statuses.poison;
            b.player.hp -= dmg;
            b.player.statuses.poison--;
            this.addLog(`毒で${dmg}ダメージ！`, 'damage');
        }

        if (b.player.hp <= 0) { this.battleDefeat(); return; }

        // Regen
        if (b.player.statuses.regen > 0) {
            const heal = b.player.statuses.regen;
            b.player.hp = Math.min(b.player.maxHP, b.player.hp + heal);
            b.player.statuses.regen--;
            this.addLog(`再生で${heal}回復`, 'heal');
        }

        // Spins
        b.spins = this.run.baseSpins + (this.run.relics.includes('extra_spin') ? 1 : 0);

        // Enemy intent
        const intent = b.intents[b.intentIndex % b.intents.length];
        b.currentIntent = intent;

        this.addLog(`--- Turn ${b.turn} (Spins: ${b.spins}) ---`, 'turn');
        this.updateBattleUI();
        this.setSpinnable(true);
    }

    updateBattleUI() {
        const b = this.battle;
        // Player
        const pFill = Math.max(0, b.player.hp / b.player.maxHP * 100);
        document.getElementById('player-hp-fill').style.width = pFill + '%';
        document.getElementById('player-hp-text').textContent = `${Math.max(0,b.player.hp)}/${b.player.maxHP}`;
        document.getElementById('player-block-text').textContent = b.player.block > 0 ? `🛡️${b.player.block}` : '';
        document.getElementById('spin-count').textContent = `Spins: ${b.spins}`;
        document.getElementById('turn-count').textContent = `Turn ${b.turn}`;

        // Statuses
        const statusDiv = document.getElementById('player-statuses');
        statusDiv.innerHTML = '';
        Object.entries(b.player.statuses).forEach(([k,v]) => {
            if (v > 0) statusDiv.innerHTML += `<span class="status-icon">${this.statusIcon(k)}${v}</span>`;
        });

        // Enemy
        const eFill = Math.max(0, b.enemy.hp / b.enemy.maxHP * 100);
        document.getElementById('enemy-hp-fill').style.width = eFill + '%';
        document.getElementById('enemy-hp-text').textContent = `${Math.max(0,b.enemy.hp)}/${b.enemy.maxHP}`;
        document.getElementById('enemy-block-text').textContent = b.enemy.block > 0 ? `🛡️${b.enemy.block}` : '';

        // Intent
        const intent = b.currentIntent;
        const intentEl = document.getElementById('enemy-intent');
        if (intent) {
            const labels = { attack:`⚔️ ${intent.value}`, defend:`🛡️ ${intent.value}`, buff:'💪 Buff', debuff:'😈 Debuff' };
            intentEl.textContent = labels[intent.type] || '???';
        }
    }

    statusIcon(type) {
        return {strength:'💪',weakness:'😰',poison:'🧪',vulnerable:'💥',regen:'💚'}[type] || '?';
    }

    setSpinnable(v) {
        document.getElementById('spin-btn').disabled = !v || this.battle.spins <= 0;
        document.getElementById('endturn-btn').disabled = !v;
    }

    spin() {
        const b = this.battle;
        if (b.spins <= 0 || b.spinning) return;

        b.spins--;
        b.spinning = true;
        b.reelsStopped = [false, false, false];
        b.reelResults = [null, null, null];

        this.setSpinnable(false);

        // Enable stop buttons
        for (let i = 0; i < 3; i++) {
            document.getElementById(`stop-btn-${i}`).disabled = false;
            document.getElementById(`reel-window-${i}`).className = 'reel-window spinning';
            document.getElementById(`reel-window-${i}`).textContent = '';
        }

        // Start animation
        b.lastTime = performance.now();
        this.spinAnimate();
    }

    spinAnimate() {
        const b = this.battle;
        const now = performance.now();
        const dt = (now - b.lastTime) / 1000;
        b.lastTime = now;

        for (let i = 0; i < 3; i++) {
            if (b.reelsStopped[i]) continue;
            b.reelTimers[i] = (b.reelTimers[i] || 0) + dt;
            const speed = this.getReelSpeed();
            if (b.reelTimers[i] >= 0.08 / speed) {
                b.reelTimers[i] = 0;
                const reel = this.run.reels[i];
                const idx = Math.floor(Math.random() * reel.length);
                const skill = reel[idx];
                document.getElementById(`reel-window-${i}`).textContent = `${skill.icon} ${skill.name}`;
            }
        }

        if (b.spinning) {
            b.animFrame = requestAnimationFrame(() => this.spinAnimate());
        }
    }

    getReelSpeed() {
        let speed = 1 + this.overclockBonus;
        if (this.aimActive) speed *= 0.2;
        return speed;
    }

    stopReel(index) {
        const b = this.battle;
        if (b.reelsStopped[index] || !b.spinning) return;

        const reel = this.run.reels[index];
        const skill = reel[Math.floor(Math.random() * reel.length)];
        b.reelResults[index] = skill;
        b.reelsStopped[index] = true;

        document.getElementById(`stop-btn-${index}`).disabled = true;
        const win = document.getElementById(`reel-window-${index}`);
        win.className = 'reel-window stopped';
        win.textContent = `${skill.icon} ${skill.name}`;

        // Check all stopped
        if (b.reelsStopped.every(s => s)) {
            b.spinning = false;
            if (b.animFrame) cancelAnimationFrame(b.animFrame);
            if (this.aimActive) this.aimActive = false;
            this.resolveEffects();
        }
    }

    resolveEffects() {
        const b = this.battle;

        for (let i = 0; i < 3; i++) {
            const skill = b.reelResults[i];
            if (!skill) continue;

            let value = skill.value;

            // Berserk multiplier for damage
            if (this.run.mechanic === 'berserk' && (skill.effect === 'deal_damage' || skill.effect === 'self_damage')) {
                value = Math.floor(value * this.getBerserkMultiplier());
            }

            // Strength bonus for damage
            if (skill.effect === 'deal_damage') {
                value += (b.player.statuses.strength || 0);
                if (b.player.statuses.weakness > 0) value -= b.player.statuses.weakness;
                value = Math.max(0, value);
            }

            switch (skill.effect) {
                case 'deal_damage': {
                    const dealt = this.dealDamageToEnemy(value);
                    this.addLog(`${skill.name}: ${dealt}ダメージ！`, 'damage');
                    this.showDamagePopup(dealt, false);
                    break;
                }
                case 'gain_block': {
                    b.player.block += value;
                    this.addLog(`${skill.name}: ブロック${value}獲得`, 'block');
                    break;
                }
                case 'heal': {
                    b.player.hp = Math.min(b.player.maxHP, b.player.hp + value);
                    this.addLog(`${skill.name}: HP${value}回復`, 'heal');
                    break;
                }
                case 'apply_poison': {
                    b.enemy.statuses.poison = (b.enemy.statuses.poison || 0) + value;
                    this.addLog(`${skill.name}: 毒${value}付与`, 'status');
                    break;
                }
                case 'apply_vulnerable': {
                    b.enemy.statuses.vulnerable = (b.enemy.statuses.vulnerable || 0) + value;
                    this.addLog(`${skill.name}: 脆弱${value}付与`, 'status');
                    break;
                }
                case 'apply_strength': {
                    b.player.statuses.strength = (b.player.statuses.strength || 0) + value;
                    this.addLog(`${skill.name}: 筋力+${value}`, 'status');
                    break;
                }
                case 'draw_spin': {
                    b.spins += value;
                    this.addLog(`${skill.name}: スピン+${value}！`, 'status');
                    break;
                }
                case 'self_damage': {
                    const selfDmg = Math.floor(skill.value / 3);
                    b.player.hp -= selfDmg;
                    this.berserkSelfDamage += selfDmg;
                    this.addLog(`${skill.name}: 自傷${selfDmg}`, 'damage');
                    this.showDamagePopup(selfDmg, true);

                    const dealt = this.dealDamageToEnemy(value);
                    this.addLog(`${skill.name}: ${dealt}ダメージ！`, 'damage');
                    this.showDamagePopup(dealt, false);
                    break;
                }
            }
        }

        // Aim gauge fill
        if (this.run.mechanic === 'aim') {
            this.aimFill = Math.min(100, this.aimFill + 25);
            this.updateAimUI();
        }
        if (this.run.mechanic === 'berserk') {
            this.updateBerserkUI();
        }

        this.updateBattleUI();

        // Check enemy death
        if (b.enemy.hp <= 0) {
            this.battleVictory();
            return;
        }

        // Check player death
        if (b.player.hp <= 0) {
            this.battleDefeat();
            return;
        }

        if (b.spins > 0) {
            this.setSpinnable(true);
        } else {
            setTimeout(() => this.executeEnemyTurn(), 600);
        }
    }

    dealDamageToEnemy(raw) {
        const b = this.battle;
        let dmg = raw;
        if (b.enemy.statuses.vulnerable > 0) dmg = Math.floor(dmg * 1.5);

        const blocked = Math.min(b.enemy.block, dmg);
        b.enemy.block -= blocked;
        const hpDmg = dmg - blocked;
        b.enemy.hp = Math.max(0, b.enemy.hp - hpDmg);

        // Shake
        document.getElementById('enemy-visual').classList.add('shake');
        setTimeout(() => document.getElementById('enemy-visual').classList.remove('shake'), 300);

        return hpDmg;
    }

    dealDamageToPlayer(raw) {
        const b = this.battle;
        let dmg = raw;
        if (b.player.statuses.vulnerable > 0) dmg = Math.floor(dmg * 1.5);

        const blocked = Math.min(b.player.block, dmg);
        b.player.block -= blocked;
        const hpDmg = dmg - blocked;
        b.player.hp = Math.max(0, b.player.hp - hpDmg);

        // Shake player HP
        document.getElementById('player-hp-bar').classList.add('shake');
        setTimeout(() => document.getElementById('player-hp-bar').classList.remove('shake'), 300);

        return hpDmg;
    }

    executeEnemyTurn() {
        const b = this.battle;
        b.enemy.block = 0;

        // Enemy poison tick
        if (b.enemy.statuses.poison > 0) {
            const dmg = b.enemy.statuses.poison;
            b.enemy.hp = Math.max(0, b.enemy.hp - dmg);
            b.enemy.statuses.poison--;
            this.addLog(`${b.enemy.name}は毒で${dmg}ダメージ！`, 'damage');
            if (b.enemy.hp <= 0) { this.battleVictory(); return; }
        }

        const intent = b.currentIntent;
        switch (intent.type) {
            case 'attack': {
                let dmg = intent.value + (b.enemy.statuses.strength || 0);
                const actual = this.dealDamageToPlayer(dmg);
                this.addLog(`${b.enemy.name}の攻撃！ ${actual}ダメージ！`, 'enemy');
                this.showDamagePopup(actual, true);
                break;
            }
            case 'defend':
                b.enemy.block += intent.value;
                this.addLog(`${b.enemy.name}はブロック${intent.value}を得た`, 'enemy');
                break;
            case 'buff':
                b.enemy.statuses.strength = (b.enemy.statuses.strength || 0) + intent.value;
                this.addLog(`${b.enemy.name}は筋力+${intent.value}！`, 'enemy');
                break;
            case 'debuff':
                b.player.statuses.weakness = (b.player.statuses.weakness || 0) + intent.value;
                this.addLog(`${b.enemy.name}は脱力${intent.value}を付与！`, 'enemy');
                break;
        }

        b.intentIndex++;

        // Tick enemy statuses
        if (b.enemy.statuses.vulnerable > 0) b.enemy.statuses.vulnerable--;

        this.updateBattleUI();

        if (b.player.hp <= 0) { this.battleDefeat(); return; }

        // Tick player statuses
        if (b.player.statuses.weakness > 0) b.player.statuses.weakness--;
        if (b.player.statuses.vulnerable > 0) b.player.statuses.vulnerable--;

        setTimeout(() => this.startPlayerTurn(), 400);
    }

    endTurn() {
        if (this.battle.spinning) return;
        this.battle.spins = 0;
        this.setSpinnable(false);
        setTimeout(() => this.executeEnemyTurn(), 300);
    }

    battleVictory() {
        this.setSpinnable(false);
        this.addLog('VICTORY!', 'turn');
        this.run.currentHP = this.battle.player.hp;

        // Check if boss
        const node = this.run.map.find(n => n.id === this.run.currentNodeId);
        if (node && node.type === 'boss') {
            setTimeout(() => {
                this.showScreen('gameover');
                document.getElementById('gameover-title').textContent = 'CONGRATULATIONS!';
                document.getElementById('gameover-info').textContent = `${this.run.characterName}でクリア！ Floor ${this.run.floor + 1} / Gold: ${this.run.gold}`;
            }, 800);
            return;
        }

        setTimeout(() => this.showReward(), 800);
    }

    battleDefeat() {
        this.setSpinnable(false);
        if (this.battle.animFrame) cancelAnimationFrame(this.battle.animFrame);
        this.addLog('DEFEAT...', 'turn');

        setTimeout(() => {
            this.showScreen('gameover');
            document.getElementById('gameover-title').textContent = 'GAME OVER';
            document.getElementById('gameover-info').textContent = `${this.run.characterName} / Floor ${this.run.floor + 1} / Turn ${this.battle.turn}`;
        }, 800);
    }

    // --- Aim Gauge ---
    activateAim() {
        if (this.aimFill < 100) return;
        this.aimActive = true;
        this.aimFill = 0;
        this.updateAimUI();
    }

    activateOverclock() {
        this.overclockBonus += 0.5;
        this.battle.player.statuses.strength = (this.battle.player.statuses.strength || 0) + 2;
        this.addLog(`オーバークロック！速度UP＋筋力+2`, 'status');
        this.updateBattleUI();
    }

    updateAimUI() {
        const fill = document.getElementById('aim-fill');
        const btn = document.getElementById('aim-btn');
        if (fill) fill.style.width = this.aimFill + '%';
        if (btn) btn.disabled = this.aimFill < 100;
    }

    // --- Berserk ---
    getBerserkMultiplier() {
        return 1.0 + this.berserkSelfDamage * 0.05;
    }

    updateBerserkUI() {
        const el = document.getElementById('berserk-info');
        if (el) el.textContent = `バーサーク倍率: x${this.getBerserkMultiplier().toFixed(2)} (自傷累計: ${this.berserkSelfDamage})`;
    }

    // --- Damage Popup ---
    showDamagePopup(amount, isPlayer) {
        const popup = document.createElement('div');
        popup.className = 'damage-popup';
        popup.textContent = amount;
        popup.style.color = isPlayer ? '#ff5252' : '#ffab40';
        popup.style.left = (isPlayer ? 30 : 60) + Math.random() * 20 + '%';
        popup.style.top = (isPlayer ? 60 : 20) + Math.random() * 10 + '%';
        document.getElementById('damage-popups').appendChild(popup);
        setTimeout(() => popup.remove(), 800);
    }

    // --- Battle Log ---
    addLog(msg, type = '') {
        const log = document.getElementById('battle-log');
        const entry = document.createElement('div');
        entry.className = `log-entry log-${type}`;
        entry.textContent = msg;
        log.appendChild(entry);
        log.scrollTop = log.scrollHeight;
    }

    // --- Reward ---
    showReward() {
        this.showScreen('reward');
        const goldReward = 20 + Math.floor(Math.random() * 15) + this.run.floor * 2;
        this.run.gold += goldReward;
        document.getElementById('gold-reward').textContent = `+${goldReward} Gold`;

        // Generate 3 rewards
        const rewards = [];
        const pool = [...SKILL_POOL];
        for (let i = 0; i < 3; i++) {
            const idx = Math.floor(Math.random() * pool.length);
            rewards.push(pool.splice(idx, 1)[0]);
        }

        const container = document.getElementById('reward-cards');
        container.innerHTML = '';
        document.getElementById('reel-assign').classList.add('hidden');

        rewards.forEach(skill => {
            const card = document.createElement('div');
            card.className = 'reward-card';
            card.innerHTML = `
                <div class="skill-name">${skill.icon} ${skill.name}</div>
                <div class="skill-desc">${skill.desc}</div>
                <span class="skill-rarity rarity-${skill.rarity}">${skill.rarity}</span>`;
            card.onclick = () => this.selectReward(skill);
            container.appendChild(card);
        });
    }

    selectReward(skill) {
        this.pendingReward = skill;
        document.getElementById('reel-assign').classList.remove('hidden');
    }

    assignToReel(index) {
        if (!this.pendingReward) return;
        this.run.reels[index].push(this.pendingReward);
        this.pendingReward = null;
        this.showMap();
    }

    skipReward() {
        this.pendingReward = null;
        this.showMap();
    }

    // --- Shop ---
    showShop() {
        this.showScreen('shop');
        document.getElementById('shop-gold').textContent = `Gold: ${this.run.gold}`;

        const container = document.getElementById('shop-items');
        container.innerHTML = '';
        document.getElementById('shop-reel-assign').classList.add('hidden');

        // Skills
        const shuffled = [...SKILL_POOL].sort(() => Math.random() - 0.5).slice(0, 3);
        shuffled.forEach(skill => {
            const cost = skill.rarity === 'rare' ? 150 : skill.rarity === 'uncommon' ? 80 : 50;
            const item = document.createElement('div');
            item.className = 'shop-item';
            item.innerHTML = `
                <div>${skill.icon} ${skill.name}</div>
                <div style="font-size:12px;color:#888">${skill.desc}</div>
                <div class="item-cost">${cost}G</div>`;
            item.onclick = () => this.buySkill(skill, cost, item);
            container.appendChild(item);
        });

        // Relics
        RELIC_SHOP.forEach(relic => {
            if (this.run.relics.includes(relic.effect)) return;
            const item = document.createElement('div');
            item.className = 'shop-item';
            item.innerHTML = `
                <div>🔮 ${relic.name}</div>
                <div style="font-size:12px;color:#888">${relic.desc}</div>
                <div class="item-cost">${relic.cost}G</div>`;
            item.onclick = () => this.buyRelic(relic, item);
            container.appendChild(item);
        });
    }

    buySkill(skill, cost, el) {
        if (this.run.gold < cost) return;
        this.run.gold -= cost;
        el.classList.add('sold');
        el.onclick = null;
        this.shopPendingSkill = skill;
        document.getElementById('shop-reel-assign').classList.remove('hidden');
        document.getElementById('shop-gold').textContent = `Gold: ${this.run.gold}`;
    }

    shopAssignToReel(index) {
        if (!this.shopPendingSkill) return;
        this.run.reels[index].push(this.shopPendingSkill);
        this.shopPendingSkill = null;
        document.getElementById('shop-reel-assign').classList.add('hidden');
    }

    buyRelic(relic, el) {
        if (this.run.gold < relic.cost) return;
        this.run.gold -= relic.cost;
        this.run.relics.push(relic.effect);
        el.classList.add('sold');
        el.onclick = null;
        document.getElementById('shop-gold').textContent = `Gold: ${this.run.gold}`;
    }

    leaveShop() { this.showMap(); }

    // --- Rest ---
    showRest() {
        this.showScreen('rest');
        document.getElementById('rest-hp').textContent = `HP: ${this.run.currentHP}/${this.run.maxHP}`;
        document.getElementById('rest-heal-btn').disabled = false;
    }

    restHeal() {
        const heal = Math.floor(this.run.maxHP * 0.3);
        this.run.currentHP = Math.min(this.run.maxHP, this.run.currentHP + heal);
        document.getElementById('rest-hp').textContent = `HP: ${this.run.currentHP}/${this.run.maxHP}`;
        document.getElementById('rest-heal-btn').disabled = true;
    }

    leaveRest() { this.showMap(); }

    // --- Restart ---
    restart() { this.showScreen('title'); }
}

// ============================================================
const game = new Game();
