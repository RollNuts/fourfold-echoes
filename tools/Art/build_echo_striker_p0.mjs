#!/usr/bin/env node
import { createHash } from "node:crypto";
import { mkdirSync, writeFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, "../..");
const modelDir = path.join(root, "Assets/Art/Production/P0/Models");
const previewDir = path.join(root, "artifacts/Previews");
const docsPreviewDir = path.join(root, "docs/Art/Previews");
const objPath = path.join(modelDir, "FE_CHAR_PLAYER_EchoStriker_P0.obj");
const mtlPath = path.join(modelDir, "FE_CHAR_PLAYER_EchoStriker_P0.mtl");
const svgPath = path.join(previewDir, "echo-striker-p0-action-poses.svg");
const gameSvgPath = path.join(previewDir, "echo-striker-p0-game-scale.svg");
const combatSvgPath = path.join(previewDir, "echo-striker-p0-combat-screen.svg");
const docsSvgPath = path.join(docsPreviewDir, "FE_CHAR_PLAYER_EchoStriker_P0_action_poses.svg");
const docsGameSvgPath = path.join(docsPreviewDir, "FE_CHAR_PLAYER_EchoStriker_P0_game_scale.svg");
const docsCombatSvgPath = path.join(docsPreviewDir, "FE_CHAR_PLAYER_EchoStriker_P0_combat_screen.svg");

const materials = {
  cloak: { hex: "#101f3d", kd: [0.063, 0.122, 0.239], stroke: "#071022" },
  cloakEdge: { hex: "#0a435a", kd: [0.039, 0.263, 0.353], stroke: "#042838" },
  tunic: { hex: "#eadfb4", kd: [0.918, 0.875, 0.706], stroke: "#877c56" },
  face: { hex: "#e1a06f", kd: [0.882, 0.627, 0.435], stroke: "#7d4f37" },
  mask: { hex: "#15121a", kd: [0.082, 0.071, 0.102], stroke: "#050408" },
  scarf: { hex: "#c51d17", kd: [0.773, 0.114, 0.090], stroke: "#640d0b" },
  gold: { hex: "#f2a522", kd: [0.949, 0.647, 0.133], stroke: "#81520e" },
  steel: { hex: "#b9d0d2", kd: [0.725, 0.816, 0.824], stroke: "#52696f" },
  dark: { hex: "#141316", kd: [0.078, 0.075, 0.086], stroke: "#050506" },
  echo: { hex: "#25d8ff", kd: [0.145, 0.847, 1.000], stroke: "#0684a8", emissive: true },
  slash: { hex: "#d9f7ff", kd: [0.851, 0.969, 1.000], stroke: "#54b8db", emissive: true },
  spark: { hex: "#ffb72b", kd: [1.000, 0.718, 0.169], stroke: "#9a5105", emissive: true },
  hurt: { hex: "#ff2a19", kd: [1.000, 0.165, 0.098], stroke: "#8a0707", emissive: true },
  shadow: { hex: "#05070b", kd: [0.020, 0.027, 0.043], stroke: "#05070b", alpha: 0.42 },
  floor: { hex: "#1a2029", kd: [0.102, 0.125, 0.161], stroke: "#273241" },
};

const r = (deg) => (deg * Math.PI) / 180;
const fmt = (n) => (Math.abs(n) < 0.000005 ? "0.00000" : n.toFixed(5));

function guid(seed) {
  return createHash("md5").update(`fourfold-echoes:${seed}`).digest("hex");
}

function meshBox() {
  return {
    vertices: [
      [-0.5, -0.5, -0.5],
      [0.5, -0.5, -0.5],
      [0.5, -0.5, 0.5],
      [-0.5, -0.5, 0.5],
      [-0.5, 0.5, -0.5],
      [0.5, 0.5, -0.5],
      [0.5, 0.5, 0.5],
      [-0.5, 0.5, 0.5],
    ],
    faces: [
      [1, 2, 3, 4],
      [5, 8, 7, 6],
      [1, 5, 6, 2],
      [2, 6, 7, 3],
      [3, 7, 8, 4],
      [4, 8, 5, 1],
    ],
  };
}

function meshWedge() {
  return {
    vertices: [
      [-0.54, -0.5, -0.35],
      [0.54, -0.5, -0.35],
      [0.62, -0.5, 0.34],
      [-0.62, -0.5, 0.34],
      [-0.28, 0.5, -0.24],
      [0.28, 0.5, -0.24],
      [0.20, 0.5, 0.24],
      [-0.20, 0.5, 0.24],
    ],
    faces: [
      [1, 2, 3, 4],
      [5, 8, 7, 6],
      [1, 5, 6, 2],
      [2, 6, 7, 3],
      [3, 7, 8, 4],
      [4, 8, 5, 1],
    ],
  };
}

function meshBlade() {
  return {
    vertices: [
      [0.0, 0.64, 0.0],
      [-0.14, -0.12, 0.07],
      [0.14, -0.12, 0.07],
      [0.11, -0.56, 0.0],
      [-0.11, -0.56, 0.0],
      [0.0, -0.12, -0.08],
    ],
    faces: [
      [1, 2, 6],
      [1, 6, 3],
      [2, 5, 6],
      [3, 6, 4],
      [2, 3, 4, 5],
      [1, 3, 2],
    ],
  };
}

function meshGem() {
  return {
    vertices: [
      [0, 0.58, 0],
      [0.46, 0, 0],
      [0, 0, 0.46],
      [-0.46, 0, 0],
      [0, 0, -0.46],
      [0, -0.58, 0],
    ],
    faces: [
      [1, 2, 3],
      [1, 3, 4],
      [1, 4, 5],
      [1, 5, 2],
      [6, 3, 2],
      [6, 4, 3],
      [6, 5, 4],
      [6, 2, 5],
    ],
  };
}

function meshSphere(segments = 10, rings = 5) {
  const vertices = [];
  for (let y = 0; y <= rings; y += 1) {
    const v = y / rings;
    const phi = -Math.PI / 2 + v * Math.PI;
    const radius = Math.cos(phi);
    for (let x = 0; x < segments; x += 1) {
      const theta = (x / segments) * Math.PI * 2;
      vertices.push([Math.cos(theta) * radius, Math.sin(phi), Math.sin(theta) * radius]);
    }
  }
  const faces = [];
  for (let y = 0; y < rings; y += 1) {
    for (let x = 0; x < segments; x += 1) {
      const a = y * segments + x + 1;
      const b = y * segments + ((x + 1) % segments) + 1;
      const c = (y + 1) * segments + ((x + 1) % segments) + 1;
      const d = (y + 1) * segments + x + 1;
      faces.push([a, b, c, d]);
    }
  }
  return { vertices, faces };
}

function meshCylinder(segments = 18) {
  const vertices = [];
  for (const y of [-0.5, 0.5]) {
    for (let i = 0; i < segments; i += 1) {
      const a = (i / segments) * Math.PI * 2;
      vertices.push([Math.cos(a), y, Math.sin(a)]);
    }
  }
  vertices.push([0, -0.5, 0], [0, 0.5, 0]);
  const bottomCenter = segments * 2 + 1;
  const topCenter = segments * 2 + 2;
  const faces = [];
  for (let i = 0; i < segments; i += 1) {
    const next = (i + 1) % segments;
    faces.push([i + 1, next + 1, segments + next + 1, segments + i + 1]);
    faces.push([bottomCenter, next + 1, i + 1]);
    faces.push([topCenter, segments + i + 1, segments + next + 1]);
  }
  return { vertices, faces };
}

function meshArc(radius, startDeg, endDeg, width, zSquash = 0.62, segments = 24) {
  const vertices = [];
  for (let i = 0; i <= segments; i += 1) {
    const t = i / segments;
    const angle = r(startDeg + (endDeg - startDeg) * t);
    for (const offset of [-width, width]) {
      const rr = radius + offset;
      vertices.push([Math.cos(angle) * rr, 0, Math.sin(angle) * rr * zSquash]);
    }
  }
  const faces = [];
  for (let i = 0; i < segments; i += 1) {
    const a = i * 2 + 1;
    faces.push([a, a + 1, a + 3, a + 2]);
  }
  return { vertices, faces };
}

function meshLine(a, b, width) {
  const dx = b[0] - a[0];
  const dz = b[2] - a[2];
  const len = Math.hypot(dx, dz) || 1;
  const nx = (-dz / len) * width;
  const nz = (dx / len) * width;
  return {
    vertices: [
      [a[0] + nx, a[1], a[2] + nz],
      [a[0] - nx, a[1], a[2] - nz],
      [b[0] - nx, b[1], b[2] - nz],
      [b[0] + nx, b[1], b[2] + nz],
    ],
    faces: [[1, 2, 3, 4]],
  };
}

const primitiveMeshes = {
  box: meshBox(),
  wedge: meshWedge(),
  blade: meshBlade(),
  gem: meshGem(),
  sphere: meshSphere(),
  cylinder: meshCylinder(),
};

function rotatePoint([x, y, z], [rx, ry, rz]) {
  let cy = Math.cos(rx);
  let sy = Math.sin(rx);
  [y, z] = [y * cy - z * sy, y * sy + z * cy];
  cy = Math.cos(ry);
  sy = Math.sin(ry);
  [x, z] = [x * cy + z * sy, -x * sy + z * cy];
  cy = Math.cos(rz);
  sy = Math.sin(rz);
  [x, y] = [x * cy - y * sy, x * sy + y * cy];
  return [x, y, z];
}

function transformPoint(point, transform) {
  const scaled = [
    point[0] * transform.scale[0],
    point[1] * transform.scale[1],
    point[2] * transform.scale[2],
  ];
  const rotated = rotatePoint(scaled, transform.rot.map(r));
  return [
    rotated[0] + transform.loc[0],
    rotated[1] + transform.loc[1],
    rotated[2] + transform.loc[2],
  ];
}

function applyRoot(point, rootTransform) {
  const rotated = rotatePoint(point, [0, r(rootTransform.yaw || 0), r(rootTransform.roll || 0)]);
  return [rotated[0] + rootTransform.x, rotated[1], rotated[2]];
}

function part(parts, type, name, mat, loc, scale, rot = [0, 0, 0]) {
  parts.push({ type, name, mat, loc, scale, rot });
}

function custom(parts, mesh, name, mat, loc = [0, 0, 0], scale = [1, 1, 1], rot = [0, 0, 0]) {
  parts.push({ type: "custom", mesh, name, mat, loc, scale, rot });
}

function addShadow(parts, stretch = 1) {
  part(parts, "cylinder", "foot_shadow", "shadow", [0, 0.02, 0], [0.58 * stretch, 0.018, 0.42 * stretch], [90, 0, 0]);
}

function addLegs(parts, left, right, leftYaw, rightYaw) {
  part(parts, "box", "left_boot", "dark", left, [0.19, 0.15, 0.35], [0, leftYaw, 0]);
  part(parts, "box", "right_boot", "dark", right, [0.19, 0.15, 0.35], [0, rightYaw, 0]);
  part(parts, "box", "left_gold_sole", "gold", [left[0], left[1] + 0.05, left[2] + 0.21], [0.13, 0.025, 0.055], [0, leftYaw, 0]);
}

function addBody(parts, leanX, leanZ, twist, crouch) {
  part(parts, "wedge", "asym_cloak_skirt", "cloak", [leanX * 0.22, 0.58 - crouch, leanZ * 0.16], [0.72, 0.66, 0.57], [0, twist * 0.3, -leanX * 18]);
  part(parts, "wedge", "bone_tunic_front", "tunic", [0.05 + leanX * 0.15, 1.00 - crouch, 0.22 + leanZ * 0.10], [0.48, 0.43, 0.30], [0, twist * 0.2, -leanX * 12]);
  part(parts, "box", "red_diagonal_chest_strap", "scarf", [-0.04 + leanX * 0.14, 1.10 - crouch, 0.39 + leanZ * 0.10], [0.34, 0.035, 0.055], [0, twist * 0.2, 28 - leanX * 10]);
  part(parts, "gem", "chest_echo_glyph", "echo", [0.13 + leanX * 0.12, 1.14 - crouch, 0.42 + leanZ * 0.10], [0.07, 0.07, 0.04], [0, twist * 0.2, 0]);
  part(parts, "wedge", "back_cape_tail", "cloakEdge", [-0.18 + leanX * 0.16, 0.69 - crouch, -0.40 + leanZ * 0.12], [0.48, 0.84, 0.24], [8, twist * 0.2, 8 - leanX * 12]);
}

function addHead(parts, leanX, faceZ) {
  part(parts, "sphere", "hood_mass", "cloak", [0.02 + leanX, 1.59, 0.00], [0.42, 0.40, 0.37]);
  part(parts, "sphere", "warm_face", "face", [leanX, 1.55, faceZ], [0.25, 0.22, 0.085]);
  part(parts, "box", "ink_eye_band", "mask", [leanX, 1.57, faceZ + 0.085], [0.19, 0.048, 0.028]);
  part(parts, "box", "left_echo_eye", "echo", [leanX - 0.055, 1.58, faceZ + 0.106], [0.035, 0.013, 0.009]);
  part(parts, "box", "right_echo_eye", "echo", [leanX + 0.055, 1.58, faceZ + 0.106], [0.035, 0.013, 0.009]);
  part(parts, "blade", "small_gold_crest", "gold", [leanX, 1.94, 0.03], [0.26, 0.32, 0.24], [86, 0, 0]);
  part(parts, "box", "hood_gold_trim", "gold", [leanX, 1.78, faceZ + 0.035], [0.25, 0.030, 0.025]);
}

function addScarf(parts, wind, lift) {
  part(parts, "wedge", "signal_scarf_knot", "scarf", [-0.08, 1.23, 0.31], [0.22, 0.12, 0.18], [0, 0, -20]);
  part(parts, "wedge", "signal_scarf_tail_a", "scarf", [-0.48 - wind * 0.08, 1.10 + lift, 0.23 - wind * 0.12], [0.16, 0.62, 0.10], [72, -10, 64 + wind * 12]);
  part(parts, "wedge", "signal_scarf_tail_b", "scarf", [-0.33 - wind * 0.06, 0.96 + lift * 0.65, 0.37 - wind * 0.08], [0.12, 0.42, 0.08], [68, -6, 44 + wind * 10]);
}

function armYaw(shoulder, hand) {
  return (Math.atan2(hand[0] - shoulder[0], hand[2] - shoulder[2]) * 180) / Math.PI;
}

function addLeftArm(parts, shoulder, hand, glowScale) {
  const mid = shoulder.map((v, i) => (v + hand[i]) * 0.5);
  const len = Math.hypot(hand[0] - shoulder[0], hand[1] - shoulder[1], hand[2] - shoulder[2]);
  const yaw = armYaw(shoulder, hand);
  part(parts, "box", "left_cloak_sleeve", "cloak", mid, [0.12, 0.11, Math.max(0.12, len * 0.42)], [0, yaw, 0]);
  part(parts, "sphere", "left_hand", "face", hand, [0.12, 0.11, 0.10]);
  part(parts, "box", "left_palm_socket", "dark", hand, [0.12, 0.10, 0.095], [0, yaw, 0]);
  part(parts, "gem", "left_palm_echo_core", "echo", [hand[0] - 0.02, hand[1] + 0.03, hand[2] + 0.02], [0.12 * glowScale, 0.12 * glowScale, 0.12 * glowScale]);
  custom(parts, meshArc(0.17 * glowScale, 20, 320, 0.010, 0.86), "left_palm_echo_ring", "echo", [hand[0], hand[1] + 0.02, hand[2] + 0.02]);
}

function addWeaponArm(parts, shoulder, hand, swordAngle, swordScale, wide = false) {
  const mid = shoulder.map((v, i) => (v + hand[i]) * 0.5);
  const len = Math.hypot(hand[0] - shoulder[0], hand[1] - shoulder[1], hand[2] - shoulder[2]);
  const yaw = armYaw(shoulder, hand);
  part(parts, "box", "right_cloak_sleeve", "cloak", mid, [0.13, 0.12, Math.max(0.12, len * 0.42)], [0, yaw, 0]);
  part(parts, "sphere", "right_hand", "face", hand, [0.13, 0.12, 0.11]);
  const sx = Math.sin(r(swordAngle));
  const sz = Math.cos(r(swordAngle));
  const grip = [hand[0] + sx * 0.14, hand[1] + 0.02, hand[2] + sz * 0.14];
  part(parts, "box", "fold_blade_grip", "dark", grip, [0.08, 0.11, 0.24], [0, swordAngle, 0]);
  part(parts, "box", "fold_blade_guard", "gold", [grip[0], grip[1] + 0.055, grip[2]], [wide ? 0.25 : 0.20, 0.055, 0.065], [0, swordAngle + 90, 0]);
  part(parts, "blade", "fold_blade", "steel", [hand[0] + sx * 0.47 * swordScale, hand[1] + 0.18, hand[2] + sz * 0.47 * swordScale], [0.24 * swordScale, 0.78 * swordScale, 0.15 * swordScale], [90, 0, -swordAngle]);
}

function addWindupEffects(parts) {
  custom(parts, meshArc(0.62, 210, 355, 0.020, 0.62), "compressed_echo_windup", "echo", [0.12, 0.82, 0.58]);
  for (let i = 0; i < 3; i += 1) {
    custom(parts, meshLine([-0.66 - i * 0.05, 0.78 + i * 0.04, 0.38], [-0.36, 0.90, 0.30 - i * 0.06], 0.014), `windup_pull_line_${i}`, "echo");
  }
}

function addSlashEffects(parts) {
  custom(parts, meshArc(1.02, -42, 166, 0.048, 0.62), "large_readable_slash_a", "slash", [0.42, 0.90, 0.38]);
  custom(parts, meshArc(0.83, -30, 148, 0.020, 0.62), "large_readable_slash_b", "echo", [0.42, 0.88, 0.38]);
  [-28, 12, 49].forEach((angle, i) => {
    const x = 1.18 + i * 0.10;
    const z = 0.16 + i * 0.18;
    custom(parts, meshLine([x, 0.78, z], [x + Math.sin(r(angle)) * 0.34, 0.88, z + Math.cos(r(angle)) * 0.34], 0.020), `impact_spark_${i}`, "spark");
  });
}

function addHurtEffects(parts) {
  [0, 45, 90, 135, 210].forEach((angle, i) => {
    custom(parts, meshLine([0.08, 1.25, 0.38], [0.08 + Math.sin(r(angle)) * 0.42, 1.30, 0.38 + Math.cos(r(angle)) * 0.42], 0.020), `hit_burst_${i}`, "hurt");
  });
}

function pose(name, label, x, build, rootTransform = {}) {
  const parts = [];
  build(parts);
  return { name, label, rootTransform: { x, ...rootTransform }, parts };
}

function buildPoses() {
  return [
    pose("IDLE", "IDLE READ", -3.8, (p) => {
      addShadow(p);
      addLegs(p, [-0.18, 0.20, 0.18], [0.22, 0.20, -0.12], -10, 8);
      addBody(p, 0.0, 0.0, 0.0, 0.0);
      addLeftArm(p, [-0.34, 1.20, 0.12], [-0.47, 1.02, 0.30], 1.0);
      addWeaponArm(p, [0.34, 1.18, 0.06], [0.55, 0.86, 0.28], 24, 0.76);
      addScarf(p, 0.0, 0.0);
      addHead(p, 0.0, 0.29);
    }),
    pose("RUN_LEAN", "RUN LEAN", -1.9, (p) => {
      addShadow(p, 1.08);
      addLegs(p, [-0.34, 0.17, 0.34], [0.34, 0.18, -0.32], -24, 22);
      addBody(p, 0.34, 0.25, -18, 0.04);
      addLeftArm(p, [-0.36, 1.10, 0.04], [-0.66, 0.92, 0.48], 0.92);
      addWeaponArm(p, [0.34, 1.12, 0.03], [0.40, 0.84, -0.34], -35, 0.74);
      addScarf(p, 1.0, 0.08);
      addHead(p, 0.13, 0.34);
      custom(p, meshLine([-0.75, 0.50, -0.38], [-0.28, 0.58, -0.20], 0.012), "run_speed_line_a", "echo");
      custom(p, meshLine([-0.86, 0.72, 0.10], [-0.44, 0.78, 0.20], 0.010), "run_speed_line_b", "slash");
    }, { roll: -7 }),
    pose("ATTACK_WINDUP", "WINDUP", 0.0, (p) => {
      addShadow(p, 1.05);
      addLegs(p, [-0.35, 0.17, 0.18], [0.40, 0.16, -0.20], -22, 20);
      addBody(p, -0.26, -0.10, 22, 0.10);
      addLeftArm(p, [-0.34, 1.02, 0.04], [-0.62, 1.05, 0.42], 1.22);
      addWeaponArm(p, [0.30, 1.06, 0.04], [-0.10, 1.30, -0.34], -116, 0.92, true);
      addScarf(p, -0.7, 0.03);
      addHead(p, -0.06, 0.30);
      addWindupEffects(p);
    }, { roll: 8 }),
    pose("ACTIVE_SLASH", "ACTIVE SLASH", 1.9, (p) => {
      addShadow(p, 1.16);
      addLegs(p, [-0.44, 0.16, -0.12], [0.48, 0.16, 0.28], -34, 34);
      addBody(p, 0.42, 0.26, -34, 0.06);
      addLeftArm(p, [-0.32, 1.08, 0.05], [-0.58, 0.96, 0.18], 1.12);
      addWeaponArm(p, [0.36, 1.10, 0.08], [0.76, 1.00, 0.54], 72, 1.03, true);
      addScarf(p, 1.3, 0.13);
      addHead(p, 0.14, 0.35);
      addSlashEffects(p);
    }, { roll: -13 }),
    pose("HIT_REACTION", "HIT REACTION", 3.8, (p) => {
      addShadow(p, 0.94);
      addLegs(p, [-0.26, 0.18, -0.22], [0.31, 0.18, 0.20], 16, -16);
      addBody(p, -0.34, -0.20, 18, 0.08);
      addLeftArm(p, [-0.36, 1.06, 0.02], [-0.64, 1.26, 0.20], 0.70);
      addWeaponArm(p, [0.32, 1.04, 0.04], [0.68, 0.82, -0.24], -12, 0.70);
      addScarf(p, -1.2, 0.10);
      addHead(p, -0.12, 0.24);
      addHurtEffects(p);
    }, { roll: 15 }),
  ];
}

function resolvedPart(partData, rootTransform) {
  const mesh = partData.type === "custom" ? partData.mesh : primitiveMeshes[partData.type];
  const transform = {
    loc: partData.loc,
    scale: partData.scale,
    rot: partData.rot,
  };
  const vertices = mesh.vertices.map((v) => applyRoot(transformPoint(v, transform), rootTransform));
  return { ...partData, vertices, faces: mesh.faces };
}

function writeObj(poses) {
  const out = [
    "# Repository-authored FOURFOLD ECHOES model candidate: FE_CHAR_PLAYER_EchoStriker_P0",
    "# Five posed groups: idle, run, attack windup, active slash, hit reaction.",
    "mtllib FE_CHAR_PLAYER_EchoStriker_P0.mtl",
    "o FE_CHAR_PLAYER_EchoStriker_P0_action_pose_sheet",
  ];
  let vertexOffset = 0;
  for (const poseData of poses) {
    out.push(`g ${poseData.name}`);
    for (const partData of poseData.parts) {
      const partMesh = resolvedPart(partData, poseData.rootTransform);
      out.push(`g ${poseData.name}_${partData.name}`);
      out.push(`usemtl ${partData.mat}`);
      for (const v of partMesh.vertices) {
        out.push(`v ${fmt(v[0])} ${fmt(v[1])} ${fmt(v[2])}`);
      }
      for (const face of partMesh.faces) {
        out.push(`f ${face.map((i) => i + vertexOffset).join(" ")}`);
      }
      vertexOffset += partMesh.vertices.length;
    }
  }
  writeFileSync(objPath, `${out.join("\n")}\n`);
}

function writeMtl() {
  const out = ["# Materials for FE_CHAR_PLAYER_EchoStriker_P0"];
  for (const [name, mat] of Object.entries(materials)) {
    out.push(`newmtl ${name}`);
    out.push(`Kd ${mat.kd.map((value) => value.toFixed(5)).join(" ")}`);
    out.push("Ka 0.05000 0.05000 0.05000");
    out.push(mat.emissive ? "Ks 0.30000 0.30000 0.30000" : "Ks 0.12000 0.12000 0.12000");
    out.push(mat.emissive ? "Ns 80" : "Ns 24");
  }
  writeFileSync(mtlPath, `${out.join("\n")}\n`);
}

function normalOf(points) {
  if (points.length < 3) return [0, 1, 0];
  const a = points[0];
  const b = points[1];
  const c = points[2];
  const u = [b[0] - a[0], b[1] - a[1], b[2] - a[2]];
  const v = [c[0] - a[0], c[1] - a[1], c[2] - a[2]];
  const n = [
    u[1] * v[2] - u[2] * v[1],
    u[2] * v[0] - u[0] * v[2],
    u[0] * v[1] - u[1] * v[0],
  ];
  const len = Math.hypot(...n) || 1;
  return n.map((x) => x / len);
}

function shade(hex, normal, emissive = false) {
  const rgb = [1, 3, 5].map((i) => Number.parseInt(hex.slice(i, i + 2), 16));
  if (emissive) return hex;
  const light = [-0.35, 0.82, -0.45];
  const ll = Math.hypot(...light);
  const dot = Math.max(0, normal[0] * (light[0] / ll) + normal[1] * (light[1] / ll) + normal[2] * (light[2] / ll));
  const factor = 0.70 + dot * 0.36;
  return `#${rgb.map((v) => Math.max(0, Math.min(255, Math.round(v * factor))).toString(16).padStart(2, "0")).join("")}`;
}

function project([x, y, z]) {
  const scale = 150;
  return {
    x: 900 + (x - z * 0.56) * scale,
    y: 660 - y * scale + z * 50 + x * 10,
  };
}

function escapeXml(value) {
  return value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

function writeSvg(poses) {
  const polygons = [];
  for (const poseData of poses) {
    for (const partData of poseData.parts) {
      const partMesh = resolvedPart(partData, poseData.rootTransform);
      const mat = materials[partData.mat];
      for (const face of partMesh.faces) {
        const vertices = face.map((index) => partMesh.vertices[index - 1]);
        const avg = vertices.reduce((sum, v) => [sum[0] + v[0], sum[1] + v[1], sum[2] + v[2]], [0, 0, 0]).map((n) => n / vertices.length);
        const screen = vertices.map(project);
        const n = normalOf(vertices);
        polygons.push({
          depth: avg[2] * 1.2 + avg[0] * 0.18 - avg[1] * 0.08,
          points: screen.map((p) => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(" "),
          fill: shade(mat.hex, n, mat.emissive),
          stroke: mat.stroke,
          alpha: mat.alpha || 1,
          glow: mat.emissive,
        });
      }
    }
  }
  polygons.sort((a, b) => a.depth - b.depth);

  const labels = poses
    .map((poseData) => {
      const x = project([poseData.rootTransform.x, 0, -1.15]).x;
      return `<text x="${x.toFixed(1)}" y="840" text-anchor="middle" class="label">${escapeXml(poseData.label)}</text>`;
    })
    .join("\n");

  const svg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1800" height="900" viewBox="0 0 1800 900">
  <defs>
    <filter id="softGlow" x="-60%" y="-60%" width="220%" height="220%">
      <feGaussianBlur stdDeviation="5" result="blur"/>
      <feMerge>
        <feMergeNode in="blur"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <linearGradient id="floorGrad" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#151b24"/>
      <stop offset="1" stop-color="#0e1218"/>
    </linearGradient>
    <style>
      .label { fill: #f2c66a; font: 700 25px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
      .caption { fill: #9fb5c1; font: 500 18px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
      .title { fill: #f4eee1; font: 800 34px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
    </style>
  </defs>
  <rect width="1800" height="900" fill="#0b0f16"/>
  <rect x="50" y="76" width="1700" height="760" rx="0" fill="url(#floorGrad)" stroke="#233142" stroke-width="2"/>
  <g opacity="0.28">
    ${[-720, -480, -240, 0, 240, 480, 720].map((x) => `<path d="M ${900 + x} 135 L ${900 + x - 250} 810" stroke="#29384a" stroke-width="2"/>`).join("\n    ")}
    ${[210, 300, 390, 480, 570, 660, 750].map((y) => `<path d="M 95 ${y} L 1705 ${y - 22}" stroke="#29384a" stroke-width="2"/>`).join("\n    ")}
  </g>
  <text x="70" y="48" class="title">FE_CHAR_PLAYER_EchoStriker_P0</text>
  <text x="70" y="73" class="caption">combat model candidate: asymmetric silhouette, weapon hand, echo-tool hand, five readable action poses</text>
  <g>
    ${polygons
      .map((poly) => `<polygon points="${poly.points}" fill="${poly.fill}" stroke="${poly.stroke}" stroke-width="${poly.glow ? 1.4 : 1}" opacity="${poly.alpha}"${poly.glow ? ' filter="url(#softGlow)"' : ""}/>`)
      .join("\n    ")}
  </g>
  ${labels}
  </svg>
`;
  writeFileSync(svgPath, svg);
  writeFileSync(docsSvgPath, svg);
}

function projectGameScale([x, y, z]) {
  const scale = 74;
  return {
    x: 640 + (x - z * 0.56) * scale,
    y: 515 - y * scale + z * 25 + x * 3,
  };
}

function writeGameScaleSvg(poses) {
  const polygons = [];
  for (const poseData of poses) {
    for (const partData of poseData.parts) {
      const partMesh = resolvedPart(partData, poseData.rootTransform);
      const mat = materials[partData.mat];
      for (const face of partMesh.faces) {
        const vertices = face.map((index) => partMesh.vertices[index - 1]);
        const avg = vertices.reduce((sum, v) => [sum[0] + v[0], sum[1] + v[1], sum[2] + v[2]], [0, 0, 0]).map((n) => n / vertices.length);
        const screen = vertices.map(projectGameScale);
        const n = normalOf(vertices);
        polygons.push({
          depth: avg[2] * 1.2 + avg[0] * 0.18 - avg[1] * 0.08,
          points: screen.map((p) => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(" "),
          fill: shade(mat.hex, n, mat.emissive),
          stroke: mat.stroke,
          alpha: mat.alpha || 1,
          glow: mat.emissive,
        });
      }
    }
  }
  polygons.sort((a, b) => a.depth - b.depth);

  const labels = poses
    .map((poseData) => {
      const x = projectGameScale([poseData.rootTransform.x, 0, -1.15]).x;
      return `<text x="${x.toFixed(1)}" y="640" text-anchor="middle" class="label">${escapeXml(poseData.label)}</text>`;
    })
    .join("\n");

  const svg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="720" viewBox="0 0 1280 720">
  <defs>
    <filter id="softGlow" x="-60%" y="-60%" width="220%" height="220%">
      <feGaussianBlur stdDeviation="3" result="blur"/>
      <feMerge>
        <feMergeNode in="blur"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <style>
      .label { fill: #f2c66a; font: 700 14px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
      .caption { fill: #9fb5c1; font: 500 15px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
      .title { fill: #f4eee1; font: 800 24px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
    </style>
  </defs>
  <rect width="1280" height="720" fill="#080c12"/>
  <rect x="44" y="86" width="1192" height="565" fill="#131a23" stroke="#263547" stroke-width="2"/>
  <g opacity="0.30">
    ${[-600, -400, -200, 0, 200, 400, 600].map((x) => `<path d="M ${640 + x} 104 L ${640 + x - 160} 635" stroke="#2a394b" stroke-width="1.6"/>`).join("\n    ")}
    ${[170, 245, 320, 395, 470, 545, 620].map((y) => `<path d="M 72 ${y} L 1208 ${y - 18}" stroke="#2a394b" stroke-width="1.6"/>`).join("\n    ")}
  </g>
  <text x="54" y="43" class="title">EchoStriker P0 game-scale readability check</text>
  <text x="54" y="68" class="caption">same OBJ pose geometry, reduced to expected top-down action-adventure screen scale</text>
  <g>
    ${polygons
      .map((poly) => `<polygon points="${poly.points}" fill="${poly.fill}" stroke="${poly.stroke}" stroke-width="${poly.glow ? 0.9 : 0.7}" opacity="${poly.alpha}"${poly.glow ? ' filter="url(#softGlow)"' : ""}/>`)
      .join("\n    ")}
  </g>
  ${labels}
</svg>
`;
  writeFileSync(gameSvgPath, svg);
  writeFileSync(docsGameSvgPath, svg);
}

function projectCombat([x, y, z]) {
  const scale = 172;
  return {
    x: 890 + (x - z * 0.58) * scale,
    y: 792 - y * scale + z * 58 + x * 10,
  };
}

function collectPosePolygons(poseData, projector) {
  const polygons = [];
  for (const partData of poseData.parts) {
    const partMesh = resolvedPart(partData, poseData.rootTransform);
    const mat = materials[partData.mat];
    for (const face of partMesh.faces) {
      const vertices = face.map((index) => partMesh.vertices[index - 1]);
      const avg = vertices.reduce((sum, v) => [sum[0] + v[0], sum[1] + v[1], sum[2] + v[2]], [0, 0, 0]).map((n) => n / vertices.length);
      const screen = vertices.map(projector);
      const n = normalOf(vertices);
      polygons.push({
        depth: avg[2] * 1.2 + avg[0] * 0.18 - avg[1] * 0.08,
        points: screen.map((p) => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(" "),
        fill: shade(mat.hex, n, mat.emissive),
        stroke: mat.stroke,
        alpha: mat.alpha || 1,
        glow: mat.emissive,
      });
    }
  }
  polygons.sort((a, b) => a.depth - b.depth);
  return polygons;
}

function enemyMarkup(x, y, scale, facing = 1, hurt = false) {
  const glow = hurt ? ' filter="url(#redGlow)"' : "";
  const core = hurt ? "#33121a" : "#1a1218";
  const armor = hurt ? "#6a2a1d" : "#302328";
  return `
    <g transform="translate(${x} ${y}) scale(${scale})">
      <ellipse cx="0" cy="72" rx="92" ry="34" fill="#03070b" opacity="0.48"/>
      <ellipse cx="0" cy="64" rx="96" ry="46" fill="none" stroke="${hurt ? "#ff3a20" : "#c83220"}" stroke-width="8" opacity="${hurt ? "0.78" : "0.38"}"${glow}/>
      <path d="M ${-46 * facing} 7 L ${-78 * facing} 63 L ${-20 * facing} 52 Z" fill="${armor}" stroke="#0b0708" stroke-width="3"/>
      <path d="M ${42 * facing} 5 L ${82 * facing} 58 L ${22 * facing} 55 Z" fill="${armor}" stroke="#0b0708" stroke-width="3"/>
      <path d="M -50 -36 L 0 -74 L 52 -36 L 42 42 L -44 42 Z" fill="${core}" stroke="#070509" stroke-width="4"/>
      <path d="M -34 -26 L 0 -51 L 36 -26 L 28 28 L -28 28 Z" fill="${armor}" stroke="#120b0e" stroke-width="3"/>
      <rect x="-25" y="-15" width="50" height="12" fill="#ff3a1d" stroke="#711008" stroke-width="3"${glow}/>
      <path d="M -18 -76 L -55 -118 L -30 -54 Z" fill="#e4441f" stroke="#711008" stroke-width="3"/>
      <path d="M 18 -76 L 55 -118 L 30 -54 Z" fill="#e4441f" stroke="#711008" stroke-width="3"/>
      ${hurt ? '<text x="0" y="-92" text-anchor="middle" class="damage">37</text>' : ""}
    </g>`;
}

function bruteEnemyMarkup(x, y, scale) {
  return `
    <g transform="translate(${x} ${y}) scale(${scale})">
      <ellipse cx="0" cy="104" rx="132" ry="42" fill="#03070b" opacity="0.52"/>
      <ellipse cx="0" cy="88" rx="138" ry="64" fill="none" stroke="#ff4a28" stroke-width="12" opacity="0.64" filter="url(#redGlow)"/>
      <path d="M -112 -8 L -188 56 L -120 83 L -68 34 Z" fill="#23161b" stroke="#0b0708" stroke-width="5"/>
      <path d="M 96 -18 L 198 50 L 138 92 L 54 35 Z" fill="#2b1b20" stroke="#0b0708" stroke-width="5"/>
      <path d="M -70 -74 L 0 -122 L 74 -76 L 89 46 L 42 112 L -48 108 L -92 43 Z" fill="#1d141b" stroke="#080509" stroke-width="6"/>
      <path d="M -48 -54 L 0 -88 L 52 -54 L 58 28 L 24 72 L -28 70 L -60 24 Z" fill="#4a241f" stroke="#180b0c" stroke-width="4"/>
      <path d="M -28 -170 L -88 -101 L -48 -82 Z" fill="#ff4a24" stroke="#771108" stroke-width="5" filter="url(#redGlow)"/>
      <path d="M 30 -170 L 90 -101 L 48 -82 Z" fill="#ff4a24" stroke="#771108" stroke-width="5" filter="url(#redGlow)"/>
      <rect x="-34" y="-34" width="68" height="18" fill="#ff351f" stroke="#7a1008" stroke-width="4" filter="url(#redGlow)"/>
      <path d="M -102 36 C -30 0, 44 4, 116 40" fill="none" stroke="#f0b64c" stroke-width="12" stroke-linecap="round" opacity="0.80" filter="url(#goldGlow)"/>
      <path d="M -154 -8 L -224 -104" stroke="#ff3824" stroke-width="9" stroke-linecap="round" filter="url(#redGlow)"/>
      <path d="M 130 0 L 226 -96" stroke="#ff3824" stroke-width="9" stroke-linecap="round" filter="url(#redGlow)"/>
      <text x="6" y="-130" text-anchor="middle" class="damage">37</text>
    </g>`;
}

function casterEnemyMarkup(x, y, scale) {
  return `
    <g transform="translate(${x} ${y}) scale(${scale})">
      <ellipse cx="0" cy="74" rx="88" ry="30" fill="#03070b" opacity="0.48"/>
      <ellipse cx="0" cy="50" rx="118" ry="48" fill="none" stroke="#ff5a28" stroke-width="8" opacity="0.34" filter="url(#redGlow)"/>
      <path d="M -52 24 L -22 -74 L 24 -74 L 58 24 L 28 70 L -28 70 Z" fill="#18131d" stroke="#080509" stroke-width="5"/>
      <path d="M -34 -44 L 0 -84 L 35 -44 L 28 16 L -28 16 Z" fill="#302033" stroke="#120914" stroke-width="4"/>
      <rect x="-24" y="-28" width="48" height="11" fill="#ff3b20" stroke="#751008" stroke-width="3" filter="url(#redGlow)"/>
      <path d="M -54 -86 L -91 -132 L -64 -66 Z" fill="#ff4a24" stroke="#771108" stroke-width="4"/>
      <path d="M 54 -86 L 91 -132 L 64 -66 Z" fill="#ff4a24" stroke="#771108" stroke-width="4"/>
      <path d="M -72 18 C -28 4, 34 4, 78 18" fill="none" stroke="#ffb33b" stroke-width="7" stroke-linecap="round" opacity="0.72" filter="url(#goldGlow)"/>
      <circle cx="102" cy="-24" r="22" fill="#ff3b20" opacity="0.86" filter="url(#redGlow)"/>
      <path d="M 62 -4 L 104 -24" stroke="#ff3b20" stroke-width="8" stroke-linecap="round" filter="url(#redGlow)"/>
    </g>`;
}

function shieldEnemyMarkup(x, y, scale) {
  return `
    <g transform="translate(${x} ${y}) scale(${scale})">
      <ellipse cx="0" cy="94" rx="126" ry="36" fill="#03070b" opacity="0.50"/>
      <ellipse cx="0" cy="73" rx="132" ry="62" fill="none" stroke="#ff4a28" stroke-width="10" opacity="0.42" filter="url(#redGlow)"/>
      <path d="M -70 -52 L 0 -100 L 72 -52 L 78 46 L 36 104 L -40 100 L -80 42 Z" fill="#1a1319" stroke="#080509" stroke-width="6"/>
      <path d="M -126 -28 L -52 -88 L -28 36 L -86 96 Z" fill="#38282f" stroke="#10090d" stroke-width="5"/>
      <path d="M -104 -12 L -64 -50 L -48 31 L -81 60 Z" fill="#70503d" stroke="#21100d" stroke-width="4"/>
      <path d="M 58 -10 L 166 -84" stroke="#ff3b20" stroke-width="12" stroke-linecap="round" filter="url(#redGlow)"/>
      <rect x="-25" y="-28" width="50" height="14" fill="#ff3a1d" stroke="#711008" stroke-width="4" filter="url(#redGlow)"/>
      <path d="M -26 -144 L -76 -84 L -42 -68 Z" fill="#ff4a24" stroke="#771108" stroke-width="5"/>
      <path d="M 30 -144 L 80 -84 L 42 -68 Z" fill="#ff4a24" stroke="#771108" stroke-width="5"/>
    </g>`;
}

function writeCombatScreenSvg(poses) {
  const active = poses.find((poseData) => poseData.name === "ACTIVE_SLASH");
  const combatHero = {
    ...active,
    rootTransform: { ...active.rootTransform, x: -0.62, roll: -15 },
  };
  const heroPolygons = collectPosePolygons(combatHero, projectCombat);

  const floorLines = [];
  for (let i = -5; i <= 9; i += 1) {
    const x = 960 + i * 176;
    floorLines.push(`<path d="M ${x} 168 L ${x - 354} 1034" stroke="#334252" stroke-width="2" opacity="0.32"/>`);
  }
  for (let i = 0; i < 9; i += 1) {
    const y = 222 + i * 92;
    floorLines.push(`<path d="M 76 ${y} L 1848 ${y - 38}" stroke="#334252" stroke-width="2" opacity="0.30"/>`);
  }

  const heroMarkup = heroPolygons
    .map((poly) => `<polygon points="${poly.points}" fill="${poly.fill}" stroke="${poly.stroke}" stroke-width="${poly.glow ? 1.3 : 1}" opacity="${poly.alpha}"${poly.glow ? ' filter="url(#cyanGlow)"' : ""}/>`)
    .join("\n      ");

  const svg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1920" height="1080" viewBox="0 0 1920 1080">
  <defs>
    <radialGradient id="arenaLight" cx="47%" cy="49%" r="70%">
      <stop offset="0" stop-color="#223024"/>
      <stop offset="0.36" stop-color="#192532"/>
      <stop offset="0.62" stop-color="#121923"/>
      <stop offset="1" stop-color="#070a0f"/>
    </radialGradient>
    <linearGradient id="stoneFace" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#39413c"/>
      <stop offset="0.52" stop-color="#222b2d"/>
      <stop offset="1" stop-color="#151a20"/>
    </linearGradient>
    <linearGradient id="routeGold" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#f0bb4d"/>
      <stop offset="1" stop-color="#81510e"/>
    </linearGradient>
    <filter id="cyanGlow" x="-70%" y="-70%" width="240%" height="240%">
      <feGaussianBlur stdDeviation="6" result="blur"/>
      <feMerge>
        <feMergeNode in="blur"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <filter id="redGlow" x="-80%" y="-80%" width="260%" height="260%">
      <feGaussianBlur stdDeviation="8" result="blur"/>
      <feMerge>
        <feMergeNode in="blur"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <filter id="goldGlow" x="-80%" y="-80%" width="260%" height="260%">
      <feGaussianBlur stdDeviation="5" result="blur"/>
      <feMerge>
        <feMergeNode in="blur"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <style>
      .damage { fill: #ffd36a; stroke: #431e08; stroke-width: 4; paint-order: stroke; font: 900 52px ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; letter-spacing: 0; }
    </style>
  </defs>
  <rect width="1920" height="1080" fill="#070a0f"/>
  <rect x="0" y="0" width="1920" height="1080" fill="url(#arenaLight)"/>
  <g opacity="0.92">
    <path d="M 82 200 L 1662 142 L 1888 960 L 72 1002 Z" fill="#111922" stroke="#2c3b47" stroke-width="4"/>
    <path d="M 138 238 L 510 212 L 480 438 L 92 470 Z" fill="url(#stoneFace)" stroke="#3d4b4d" stroke-width="3" opacity="0.74"/>
    <path d="M 522 210 L 912 194 L 882 430 L 494 438 Z" fill="#172225" stroke="#354447" stroke-width="3" opacity="0.68"/>
    <path d="M 924 194 L 1302 178 L 1282 416 L 896 430 Z" fill="#1f2b26" stroke="#384743" stroke-width="3" opacity="0.72"/>
    <path d="M 1318 178 L 1642 170 L 1704 402 L 1296 416 Z" fill="#172126" stroke="#354248" stroke-width="3" opacity="0.70"/>
    <path d="M 92 486 L 480 452 L 460 706 L 48 742 Z" fill="#172028" stroke="#34424c" stroke-width="3" opacity="0.72"/>
    <path d="M 494 452 L 884 440 L 872 694 L 468 706 Z" fill="#1f2a28" stroke="#3b4b4d" stroke-width="3" opacity="0.76"/>
    <path d="M 900 440 L 1288 424 L 1298 678 L 884 694 Z" fill="#17212c" stroke="#364555" stroke-width="3" opacity="0.72"/>
    <path d="M 1304 424 L 1712 410 L 1784 660 L 1314 678 Z" fill="#1a2422" stroke="#394746" stroke-width="3" opacity="0.72"/>
    <path d="M 48 760 L 460 722 L 452 970 L 20 986 Z" fill="#151e25" stroke="#303f48" stroke-width="3" opacity="0.70"/>
    <path d="M 474 722 L 874 710 L 872 950 L 462 970 Z" fill="#1a2524" stroke="#394746" stroke-width="3" opacity="0.74"/>
    <path d="M 890 710 L 1302 692 L 1324 930 L 884 950 Z" fill="#151f28" stroke="#334250" stroke-width="3" opacity="0.72"/>
    <path d="M 1318 692 L 1794 676 L 1890 908 L 1338 930 Z" fill="#1f2524" stroke="#404846" stroke-width="3" opacity="0.76"/>
  </g>
  <g>
    ${floorLines.join("\n    ")}
  </g>
  <g opacity="0.78">
    <path d="M 350 628 C 585 528, 792 560, 1018 442 S 1438 300, 1744 372" fill="none" stroke="url(#routeGold)" stroke-width="28" opacity="0.20"/>
    <path d="M 350 628 C 585 528, 792 560, 1018 442 S 1438 300, 1744 372" fill="none" stroke="#ffe08b" stroke-width="5" opacity="0.32" filter="url(#goldGlow)"/>
    <path d="M 300 678 C 520 605, 704 650, 920 540" fill="none" stroke="#4f7f68" stroke-width="14" opacity="0.18"/>
  </g>
  <g opacity="0.78">
    <path d="M 286 808 L 392 778 L 414 844 L 304 878 Z" fill="#2e3939" stroke="#627068" stroke-width="4"/>
    <path d="M 1514 210 L 1702 242 L 1688 292 L 1502 262 Z" fill="#2a3439" stroke="#66706c" stroke-width="4"/>
    <path d="M 1424 866 L 1622 836 L 1640 924 L 1442 954 Z" fill="#2b3636" stroke="#687269" stroke-width="4"/>
    <path d="M 422 276 L 576 318 L 552 368 L 400 330 Z" fill="#2b3538" stroke="#626f6b" stroke-width="4"/>
    <path d="M 1028 216 L 1078 300 L 1018 334 L 972 254 Z" fill="#4f4330" stroke="#867044" stroke-width="4" opacity="0.72"/>
    <path d="M 1010 248 L 1050 310" stroke="#f3b94c" stroke-width="5" opacity="0.34" filter="url(#goldGlow)"/>
  </g>
  <g opacity="0.55">
    <path d="M 606 300 L 694 340 L 620 372 Z" fill="#2d5c44"/>
    <path d="M 722 278 L 786 306 L 730 334 Z" fill="#315f46"/>
    <path d="M 144 616 L 220 640 L 154 670 Z" fill="#315f46"/>
    <path d="M 1292 272 L 1352 292 L 1308 326 Z" fill="#2f6044"/>
  </g>
  <g opacity="0.82">
    <ellipse cx="1402" cy="477" rx="220" ry="84" fill="none" stroke="#f13b22" stroke-width="10" opacity="0.46" filter="url(#redGlow)"/>
    <ellipse cx="1580" cy="738" rx="260" ry="98" fill="none" stroke="#f13b22" stroke-width="12" opacity="0.42" filter="url(#redGlow)"/>
    <path d="M 1474 444 L 1562 392 L 1520 518 Z" fill="#f13b22" opacity="0.36" filter="url(#redGlow)"/>
  </g>
  ${bruteEnemyMarkup(1336, 608, 0.95)}
  ${casterEnemyMarkup(1538, 405, 0.74)}
  ${shieldEnemyMarkup(1634, 790, 1.03)}
  <g>
      ${heroMarkup}
  </g>
  <g filter="url(#cyanGlow)" opacity="0.96">
    <path d="M 686 660 C 900 762, 1188 744, 1410 610" fill="none" stroke="#e9fdff" stroke-width="18" stroke-linecap="round"/>
    <path d="M 708 635 C 936 722, 1184 704, 1388 584" fill="none" stroke="#20d8ff" stroke-width="6" stroke-linecap="round"/>
    <path d="M 1244 552 L 1444 486" stroke="#ffd266" stroke-width="11" stroke-linecap="round"/>
    <path d="M 1272 622 L 1462 664" stroke="#ff3a20" stroke-width="9" stroke-linecap="round"/>
    <path d="M 1178 532 L 1310 430" stroke="#ff3a20" stroke-width="7" stroke-linecap="round"/>
  </g>
  <g>
    <path d="M 44 42 L 330 42 L 360 73 L 336 114 L 44 114 Z" fill="#0a1017" stroke="#6b5131" stroke-width="4"/>
    <path d="M 64 58 L 306 58 L 320 72 L 308 86 L 64 86 Z" fill="#2b1015" stroke="#77313a" stroke-width="3"/>
    <path d="M 70 64 L 282 64 L 282 80 L 70 80 Z" fill="#e43a28"/>
    <path d="M 64 94 L 250 94 L 260 103 L 250 112 L 64 112 Z" fill="#092e38" stroke="#23899c" stroke-width="3"/>
    <path d="M 70 99 L 218 99 L 218 107 L 70 107 Z" fill="#25d8ff" filter="url(#cyanGlow)"/>
    <circle cx="394" cy="80" r="38" fill="#101f3d" stroke="#f2a522" stroke-width="6" filter="url(#goldGlow)"/>
    <path d="M 394 47 L 416 80 L 394 114 L 372 80 Z" fill="#25d8ff" filter="url(#cyanGlow)"/>
  </g>
  <g opacity="0.72">
    <path d="M 34 990 L 1886 990 L 1886 1022 L 34 1022 Z" fill="#05070b"/>
    <path d="M 572 1002 L 1348 1002 L 1364 1011 L 1348 1020 L 572 1020 L 556 1011 Z" fill="#0b1720" stroke="#355466" stroke-width="2"/>
    <path d="M 820 1004 L 1098 1004 L 1110 1011 L 1098 1018 L 820 1018 L 808 1011 Z" fill="#25d8ff" filter="url(#cyanGlow)"/>
  </g>
</svg>
`;
  writeFileSync(combatSvgPath, svg);
  writeFileSync(docsCombatSvgPath, svg);
}

function writeMetas() {
  const modelDirMeta = path.join(root, "Assets/Art/Production/P0/Models.meta");
  writeFileSync(modelDirMeta, `fileFormatVersion: 2
guid: ${guid("Assets/Art/Production/P0/Models")}
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`);

  writeFileSync(`${mtlPath}.meta`, `fileFormatVersion: 2
guid: ${guid("Assets/Art/Production/P0/Models/FE_CHAR_PLAYER_EchoStriker_P0.mtl")}
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`);

  writeFileSync(`${objPath}.meta`, `fileFormatVersion: 2
guid: ${guid("Assets/Art/Production/P0/Models/FE_CHAR_PLAYER_EchoStriker_P0.obj")}
ModelImporter:
  serializedVersion: 24200
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 2
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    removeConstantScaleCurves: 0
    motionNodeName: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 0
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 1
    importPhysicalCameras: 0
    importVisibility: 0
    importBlendShapes: 1
    importCameras: 0
    importLights: 0
    nodeNameCollisionStrategy: 1
    fileIdsGeneration: 2
    swapUVChannels: 0
    generateSecondaryUV: 0
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    bakeAxisConversion: 0
    preserveHierarchy: 0
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    optimizeBones: 1
    generateMeshLods: 0
    meshLodGenerationFlags: 0
    maximumMeshLod: -1
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVMarginMethod: 1
    secondaryUVMinLightmapResolution: 40
    secondaryUVMinObjectScale: 1
    secondaryUVPackMargin: 4
    useFileScale: 1
    strictVertexDataChecks: 0
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 1
    normalSmoothingSource: 0
  referencedClips: []
  importAnimation: 0
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legStretch: 0.05
    armStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {instanceID: 0}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 2
  humanoidOversampling: 1
  avatarSetup: 0
  addHumanoidExtraRootOnlyWhenUsingAvatar: 1
  importBlendShapeDeformPercent: 1
  remapMaterialsIfMaterialImportModeIsNone: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`);
}

function main() {
  mkdirSync(modelDir, { recursive: true });
  mkdirSync(previewDir, { recursive: true });
  mkdirSync(docsPreviewDir, { recursive: true });
  const poses = buildPoses();
  writeMtl();
  writeObj(poses);
  writeSvg(poses);
  writeGameScaleSvg(poses);
  writeCombatScreenSvg(poses);
  writeMetas();
  console.log(`Wrote ${path.relative(root, objPath)}`);
  console.log(`Wrote ${path.relative(root, mtlPath)}`);
  console.log(`Wrote ${path.relative(root, svgPath)}`);
  console.log(`Wrote ${path.relative(root, gameSvgPath)}`);
  console.log(`Wrote ${path.relative(root, combatSvgPath)}`);
  console.log(`Wrote ${path.relative(root, docsSvgPath)}`);
  console.log(`Wrote ${path.relative(root, docsGameSvgPath)}`);
  console.log(`Wrote ${path.relative(root, docsCombatSvgPath)}`);
}

main();
