"use strict";

const DATA_ROOT = "TeaMobi/DuLieu";
const PAGE_SIZE = 36;

const CLASS_NAMES = ["Kiếm khách", "Chiến binh", "Pháp sư", "Đấu sĩ", "Cung thủ"];
// ItemTemplate.nameItem của client: tên loại trang bị theo trường "loai".
const ITEM_TYPES = ["Áo", "Quần", "Nón", "Kiếm", "Đao", "Bút", "Búa", "Cung", "Nhẫn", "Dây chuyền",
  "Giày", "Găng tay", "Ngọc bội", "Cuốc", "Giáp linh thú", "Hộ uyển", "Nón linh thú", "Bàn đạp", "Yên cương", "Phi phong"];
const FIELD_LABELS = {
  id: "ID", iconId: "ID ảnh", anhId: "ID ảnh gốc", ten: "Tên", moTa: "Mô tả", gia: "Giá",
  loai: "Loại", kieuDang: "Kiểu dáng", he: "Hệ", gioiTinh: "Giới tính", cap: "Cấp yêu cầu",
  doBen: "Độ bền", chiSoGoc: "Chỉ số gốc", lop: "Phái", mau: "Màu", soNgayThue: "Số ngày thuê",
  hoiChieuMs: "Hồi chiêu (ms)", coTheGiaoDich: "Có thể giao dịch", hoiPhuc: "Hồi phục",
  coTheBan: "Có thể bán", kieuEp: "Kiểu ép", loaiTien: "Loại tiền", loaiShop: "Loại shop",
  giaTri: "Giá trị", hp: "HP", kieuDiChuyen: "Kiểu di chuyển", tocDo: "Tốc độ",
  khungRong: "Rộng khung", khungCao: "Cao khung", tamX: "Tâm X", tamY: "Tâm Y",
  palate: "Palate", sPalate: "Bảng màu", quaiMoi: "Quái kiểu mới", anh: "Kiểu ảnh",
  giaHoc: "Giá học", tamDanh: "Tầm đánh", danhLan: "Đánh lan", cacCap: "Các cấp"
};

const ICON_BOX = 66;        // cạnh lớn nhất của icon trong thẻ
const ICON_BOX_LARGE = 88;  // trong panel chi tiết
const SPRITE_BOX = 72;
const SPRITE_BOX_LARGE = 92;
const SKILL_FRAME = 20;     // GameData.saveImgSkill: bảng icon kỹ năng khung 20×20
const reduceMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)");

function iconImage(value) {
  const id = numberOrNull(value);
  return id === null || id < 0 ? null : id;
}

const CATEGORIES = {
  equipment: {
    code: "TB", title: "Trang bị", short: "Trang bị", file: "EquipmentItems.json", layout: "cards",
    description: "Mẫu trang bị: phái sử dụng, loại, cấp yêu cầu, độ bền và giá.",
    image: (x) => iconImage(x.iconId),
    detail: (x) => `${itemType(x.loai)} · độ bền ${formatNumber(x.doBen)}`,
    tags: (x) => [className(x.lop), itemType(x.loai), Number(x.cap) > 0 ? `cấp ${x.cap}` : ""],
    metric: (x) => Number(x.gia) > 0 ? `${formatNumber(x.gia)} xu` : ""
  },
  potions: {
    code: "BT", title: "Bình thuốc", short: "Bình thuốc", file: "Potions.json", layout: "cards",
    description: "Bình hồi HP, MP và vật phẩm tiêu hao, lượng hồi phục và thời gian hồi.",
    image: (x) => iconImage(x.iconId),
    detail: (x) => x.moTa || `Hồi ${formatNumber(x.hoiPhuc)}`,
    tags: (x) => [`hồi ${formatNumber(x.hoiPhuc)}`, tradeLabel(x.coTheGiaoDich)],
    metric: (x) => Number(x.gia) > 0 ? `${formatNumber(x.gia)} xu` : ""
  },
  gems: {
    code: "NG", title: "Ngọc và đá", short: "Ngọc", file: "Gems.json", layout: "cards",
    description: "Ngọc khảm, đá nâng cấp và nguyên liệu ép đồ từ bảng vật phẩm máy chủ.",
    image: (x) => iconImage(x.iconId),
    detail: (x) => x.moTa,
    tags: (x) => [`loại ${x.loai}`, x.coTheBan ? "bán được" : "không bán"],
    metric: (x) => Number(x.gia) > 0 ? formatNumber(x.gia) : ""
  },
  shop: {
    code: "CH", title: "Vật phẩm cửa hàng", short: "Cửa hàng", file: "ShopItems.json", layout: "cards",
    description: "Vật phẩm bán trong cửa hàng, loại tiền và giá trị sử dụng.",
    image: (x) => iconImage(x.iconId),
    detail: (x) => x.moTa,
    tags: (x) => [`shop ${x.loaiShop}`, x.coTheBan ? "bán được" : "không bán"],
    metric: (x) => Number(x.gia) > 0 ? formatNumber(x.gia) : ""
  },
  monsters: {
    code: "Q", title: "Quái", short: "Quái", file: "Monsters.json", layout: "cards",
    description: "Mẫu quái gồm HP, hệ, kiểu di chuyển và hoạt ảnh lấy từ máy chủ.",
    // Ảnh quái thường là dải dọc các khung rộng × cao của mẫu; client đi bộ hướng 0 qua khung 0–3.
    image: (x) => x.anh ? {
      src: `${DATA_ROOT}/Monsters/${x.id}.png`, kind: x.anh, fw: x.khungRong, fh: x.khungCao
    } : null,
    detail: (x) => `${formatNumber(x.hp)} HP · hệ ${x.he}`,
    tags: (x) => [`hệ ${x.he}`, `di chuyển ${x.kieuDiChuyen}`],
    metric: (x) => `${formatNumber(x.hp)} HP`
  },
  skills: {
    code: "KN", title: "Kỹ năng", short: "Kỹ năng", file: "Skills.json", layout: "cards", pageSize: 200,
    description: "Kỹ năng của 5 phái. Chỉ số từng cấp có ở các phái đã có nhân vật đăng nhập.",
    image: (x) => ({ src: `${DATA_ROOT}/SkillIcons/${x.lop}.png`, kind: "skill", frame: x.id }),
    group: (x) => skillClass(x.lop),
    groupOrder: (x) => (Number(x.lop) >= 0 ? Number(x.lop) : 99),
    detail: (x) => x.moTa || (x.cacCap?.length ? `${x.cacCap.length} cấp kỹ năng` : "Chưa có chỉ số cấp"),
    tags: (x) => [skillClass(x.lop), x.danhLan ? "đánh lan" : "", Number(x.tamDanh) > 0 ? `tầm ${x.tamDanh}` : "",
      Number(x.giaHoc) > 0 ? `học ${formatNumber(x.giaHoc)}` : ""],
    metric: (x) => x.cacCap?.length ? `${x.cacCap.length} cấp` : ""
  }
};

const state = {
  category: "equipment",
  query: "",
  sort: "id-asc",
  page: 0,
  data: [],
  filtered: [],
  selected: null,
  cache: new Map(),
  request: 0,
  images: null,
  invoker: null,
  hasCategory: false
};

const el = (id) => document.getElementById(id);
const routeContainer = el("routes");
const grid = el("record-grid");
const pagination = el("pagination");
const searchInput = el("search");
const sortSelect = el("sort");

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, (char) => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
  })[char]);
}

function normalize(value) {
  return String(value ?? "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/đ/g, "d").replace(/Đ/g, "D").toLowerCase();
}

function numberOrNull(value) {
  return value === null || value === undefined || value === "" || Number.isNaN(Number(value)) ? null : Number(value);
}

function recordId(record) {
  return record.id ?? record.iconId ?? 0;
}

function recordName(record) {
  return record.ten || record.name || `Mục #${recordId(record)}`;
}

function formatNumber(value) {
  const number = Number(value);
  return Number.isFinite(number) ? new Intl.NumberFormat("vi-VN").format(number) : String(value ?? "—");
}

function className(value) {
  const number = Number(value);
  if (number < 0 || !Number.isFinite(number)) return "Dùng chung";
  return CLASS_NAMES[number] || `Lớp ${number}`;
}

function itemType(value) {
  const number = Number(value);
  return ITEM_TYPES[number] || `loại ${value}`;
}

function skillClass(value) {
  const number = Number(value);
  return Number.isFinite(number) && number >= 0 ? className(number) : "Chưa rõ phái";
}

function tradeLabel(value) {
  return value === true || Number(value) > 0 ? "giao dịch được" : "không giao dịch";
}

function compactText(value, fallback = "Không có mô tả") {
  const text = String(value ?? "").replace(/\s+/g, " ").trim();
  return text || fallback;
}

function iconPath(id, extension = "png") {
  return `${DATA_ROOT}/Icons/${Math.floor(Number(id) / 1000)}/${id}.${extension}`;
}

// image: số = id icon (Icons/…); object = { src, kind, … } cho ảnh quái ("strip"/"obj") và icon kỹ năng.
function iconMarkup(image, label, large = false) {
  const frameClass = `icon-frame${large ? " large" : ""}`;
  if (image === null || image === undefined || (typeof image === "number" && state.images && !state.images.has(image))) {
    return `<span class="${frameClass} empty" aria-hidden="true"></span>`;
  }
  if (typeof image === "number") {
    return `<span class="${frameClass}"><img src="${iconPath(image)}" data-icon-id="${image}" data-ext-index="0" alt="${escapeHtml(label)}" loading="lazy"></span>`;
  }
  const data = Object.entries(image).filter(([key]) => key !== "src")
    .map(([key, value]) => ` data-${key}="${escapeHtml(value)}"`).join("");
  return `<span class="${frameClass}"><img src="${escapeHtml(image.src)}"${data} alt="${escapeHtml(label)}" loading="lazy"></span>`;
}

// Ảnh game rất nhỏ: phóng theo bội số nguyên để pixel art giữ nét; ảnh lớn hơn khung thì thu nhỏ.
function fitScale(width, height, box) {
  const side = Math.max(width, height);
  return side > box ? box / side : Math.max(1, Math.floor(box / side));
}

function setSpriteFrame(node, tick) {
  const frames = Number(node.dataset.frames);
  const frame = Number(node.dataset.start || 0) + (frames > 1 ? tick % frames : 0);
  node.style.backgroundPosition = `0 ${-frame * Number(node.dataset.fh) * Number(node.dataset.scale)}px`;
}

function frameEmpty(image) {
  image.parentElement?.classList.add("empty");
  image.remove();
}

// Chọn khung theo đúng cách client vẽ:
// - icon (GameData.paintIcon): cao hơn 25px là dải dọc 3 khung chạy vòng;
// - quái "strip": dải dọc khung rộng × cao của mẫu, đi bộ qua khung 0–3;
// - kỹ năng: khung 20×20 thứ "frame" trong bảng icon của phái.
function spriteLayout(image, width, height) {
  const { kind } = image.dataset;
  if (kind === "skill") {
    const frame = Number(image.dataset.frame);
    if (!(frame >= 0) || (frame + 1) * SKILL_FRAME > height) return null;
    return { fw: Math.min(width, SKILL_FRAME), fh: SKILL_FRAME, frames: 1, start: frame };
  }
  if (kind === "strip") {
    const fw = Number(image.dataset.fw);
    const fh = Number(image.dataset.fh);
    if (!(fw > 0 && fh > 0 && fh <= height)) return { fw: width, fh: height, frames: 1, start: 0 };
    return { fw: Math.min(fw, width), fh, frames: Math.max(1, Math.min(4, Math.floor(height / fh))), start: 0 };
  }
  if (!kind && height > 25) return { fw: width, fh: Math.floor(height / 3), frames: 3, start: 0 };
  return undefined;
}

function sizeImage(image) {
  const width = image.naturalWidth;
  const height = image.naturalHeight;
  if (!width || !height) return;
  const large = Boolean(image.closest(".icon-frame.large"));
  const layout = spriteLayout(image, width, height);
  if (layout === null) {
    frameEmpty(image);
    return;
  }
  if (!layout) {
    const scale = fitScale(width, height, large ? ICON_BOX_LARGE : ICON_BOX);
    image.style.width = `${width * scale}px`;
    image.style.height = `${height * scale}px`;
    return;
  }
  const isSprite = image.dataset.kind === "strip";
  const scale = fitScale(layout.fw, layout.fh, isSprite ? (large ? SPRITE_BOX_LARGE : SPRITE_BOX) : (large ? ICON_BOX_LARGE : ICON_BOX));
  const node = document.createElement("span");
  node.className = "sprite";
  node.setAttribute("role", "img");
  node.setAttribute("aria-label", image.alt);
  Object.assign(node.dataset, { fh: layout.fh, frames: layout.frames, start: layout.start, scale });
  Object.assign(node.style, {
    width: `${layout.fw * scale}px`,
    height: `${layout.fh * scale}px`,
    backgroundImage: `url("${image.src}")`,
    backgroundSize: `${width * scale}px ${height * scale}px`
  });
  setSpriteFrame(node, 0);
  image.replaceWith(node);
}

function mountImageFallbacks(root = document) {
  const extensions = ["png", "gif", "jpg"];
  root.querySelectorAll(".icon-frame img").forEach((image) => {
    image.addEventListener("error", () => {
      if (image.dataset.iconId === undefined) {
        frameEmpty(image);
        return;
      }
      const next = Number(image.dataset.extIndex || 0) + 1;
      if (next < extensions.length) {
        image.dataset.extIndex = String(next);
        image.src = iconPath(image.dataset.iconId, extensions[next]);
      } else {
        frameEmpty(image);
      }
    });
    if (image.complete && image.naturalWidth) sizeImage(image);
    else image.addEventListener("load", () => sizeImage(image));
  });
}

// Một bộ đếm chung cho mọi ảnh động đang hiển thị.
let spriteTick = 0;
setInterval(() => {
  if (reduceMotion?.matches || document.hidden) return;
  spriteTick += 1;
  document.querySelectorAll(".sprite").forEach((node) => {
    if (Number(node.dataset.frames) > 1) setSpriteFrame(node, spriteTick);
  });
}, 220);

async function fetchJson(file) {
  if (state.cache.has(file)) return state.cache.get(file);
  const response = await fetch(`${DATA_ROOT}/${file}`, { cache: "no-store" });
  if (!response.ok) throw new Error(`HTTP ${response.status}`);
  const data = await response.json();
  if (!Array.isArray(data)) throw new Error("Dữ liệu không phải danh sách");
  state.cache.set(file, data);
  return data;
}

function renderRoutes() {
  routeContainer.innerHTML = Object.entries(CATEGORIES).map(([key, category]) => `
    <button class="route-button" type="button" data-category="${key}"
      aria-pressed="${key === state.category}">${escapeHtml(category.short)}</button>`).join("");
}

// Chỉ cuộn ngang dải danh mục; scrollIntoView sẽ kéo cả trang xuống dưới header dính.
function centerRoute(key) {
  const button = routeContainer.querySelector(`[data-category="${key}"]`);
  if (!button || routeContainer.scrollWidth <= routeContainer.clientWidth) return;
  const offset = button.getBoundingClientRect().left - routeContainer.getBoundingClientRect().left + routeContainer.scrollLeft;
  const left = offset - (routeContainer.clientWidth - button.offsetWidth) / 2;
  routeContainer.scrollTo({ left: Math.max(0, left), behavior: "auto" });
}

function loadingState() {
  grid.setAttribute("aria-busy", "true");
  grid.innerHTML = `<div class="loading-row" aria-label="Đang tải dữ liệu"><i></i><i></i><i></i><i></i><i></i><i></i></div>`;
  pagination.innerHTML = "";
}

function errorState(category, error) {
  grid.setAttribute("aria-busy", "false");
  const firstRun = /404/.test(error.message);
  grid.innerHTML = `<div class="empty-state"><div>
    <span class="empty-code">${firstRun ? "CHỜ" : "LỖI"}</span>
    <h2>${firstRun ? "Dữ liệu đang được lấy lần đầu" : "Không đọc được dữ liệu"}</h2>
    <p>${firstRun
      ? `File <strong>${escapeHtml(category.file)}</strong> chưa xuất hiện. Workflow đang đăng nhập nhân vật KPAH và sẽ tự đưa dữ liệu lên đây.`
      : `${escapeHtml(error.message)}. Bạn có thể thử lại hoặc kiểm tra lượt chạy GitHub Actions.`}</p>
    <div class="empty-actions">
      <button class="button" type="button" data-retry>Thử lại</button>
      <a class="button secondary" href="https://github.com/duykhongphai/DataKpah/actions" target="_blank" rel="noreferrer">Xem Actions</a>
    </div>
  </div></div>`;
  el("record-count").textContent = "0";
  el("result-note").textContent = "Chưa có dữ liệu";
  pagination.innerHTML = "";
}

async function loadCategory(key, updateHash = true) {
  if (!CATEGORIES[key]) key = "equipment";
  const category = CATEGORIES[key];
  state.category = key;
  state.page = 0;
  state.selected = null;
  closeDetail(false);
  renderRoutes();
  el("route-code").textContent = category.code;
  el("route-name").textContent = category.title;
  el("stage-title").textContent = category.title;
  el("stage-description").textContent = category.description;
  if (state.hasCategory) {
    const routeCurrent = document.querySelector(".route-current");
    const stageHeading = document.querySelector(".stage-heading");
    routeCurrent.classList.remove("route-swap");
    stageHeading.classList.remove("route-swap");
    void routeCurrent.offsetWidth;
    routeCurrent.classList.add("route-swap");
    stageHeading.classList.add("route-swap");
  }
  state.hasCategory = true;
  el("raw-link").href = `${DATA_ROOT}/${category.file}`;
  if (updateHash && location.hash !== `#/${key}`) history.pushState(null, "", `#/${key}`);
  loadingState();
  const request = ++state.request;
  try {
    const data = await fetchJson(category.file);
    if (request !== state.request) return;
    state.data = data;
    applyFilters();
    centerRoute(key);
  } catch (error) {
    if (request !== state.request) return;
    state.data = [];
    state.filtered = [];
    errorState(category, error);
  }
}

function applyFilters(resetPage = false) {
  if (resetPage) state.page = 0;
  const query = normalize(state.query.trim());
  state.filtered = state.data.filter((record) => !query || normalize(JSON.stringify(record)).includes(query));
  const [field, direction] = state.sort.split("-");
  const category = CATEGORIES[state.category];
  state.filtered.sort((a, b) => {
    // Danh mục có nhóm (kỹ năng theo phái) luôn xếp theo nhóm trước, rồi mới theo lựa chọn.
    if (category.groupOrder) {
      const grouped = category.groupOrder(a) - category.groupOrder(b);
      if (grouped) return grouped;
    }
    const left = field === "name" ? normalize(recordName(a)) : Number(recordId(a));
    const right = field === "name" ? normalize(recordName(b)) : Number(recordId(b));
    const compared = typeof left === "string" ? left.localeCompare(right, "vi") : left - right;
    return direction === "desc" ? -compared : compared;
  });
  renderRecords();
}

function recordTags(category, record) {
  return (category.tags(record) || []).filter((tag) => tag !== null && tag !== undefined && tag !== "")
    .slice(0, 4).map((tag) => `<span class="tag">${escapeHtml(tag)}</span>`).join("");
}

function copyIcon() {
  return `<svg aria-hidden="true" viewBox="0 0 24 24"><rect x="8" y="8" width="12" height="12" rx="2"/><path d="M16 8V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h2"/></svg>`;
}

function renderCard(category, record, absoluteIndex) {
  const id = recordId(record);
  const name = recordName(record);
    return `<article class="record-card" role="button" tabindex="0" data-index="${absoluteIndex}" aria-selected="false">
    <div class="card-main">
      ${iconMarkup(category.image(record), name)}
      <div class="card-copy">
        <h2 class="card-title">${escapeHtml(name)}</h2>
        <div class="card-description">${escapeHtml(compactText(category.detail(record)))}</div>
        <div class="tags">${recordTags(category, record)}</div>
      </div>
    </div>
    <div class="card-foot">
      <button class="id-chip" type="button" data-copy="${escapeHtml(id)}" title="Chép ID">${copyIcon()} ID ${escapeHtml(id)}</button>
      <span class="card-metric">${escapeHtml(category.metric(record) || "")}</span>
    </div>
  </article>`;
}

function renderTable(category, records, start) {
  const rows = records.map((record, index) => `<tr tabindex="0" data-index="${start + index}">
    <td class="mono"><button class="id-chip" type="button" data-copy="${escapeHtml(recordId(record))}">${copyIcon()} ${escapeHtml(recordId(record))}</button></td>
    <td><strong>${escapeHtml(recordName(record))}</strong></td>
    <td>${escapeHtml(compactText(category.detail(record), "—"))}</td>
    <td class="number">${escapeHtml(category.metric(record) || "")}</td>
  </tr>`).join("");
  return `<div class="table-shell"><table class="data-table">
    <thead><tr><th>ID</th><th>Tên</th><th>Chi tiết</th><th class="number">Giá trị</th></tr></thead>
    <tbody>${rows}</tbody>
  </table></div>`;
}

function renderGroups(category, records, start) {
  const groups = [];
  records.forEach((record, index) => {
    const name = category.group(record);
    if (groups.at(-1)?.name !== name) groups.push({ name, cards: [] });
    groups.at(-1).cards.push(renderCard(category, record, start + index));
  });
  return groups.map((group) => `<section class="record-group" aria-label="${escapeHtml(group.name)}">
    <h2 class="group-title"><span>${escapeHtml(group.name)}</span><small>${formatNumber(group.cards.length)} kỹ năng</small></h2>
    <div class="card-grid">${group.cards.join("")}</div>
  </section>`).join("");
}

function renderRecords() {
  const category = CATEGORIES[state.category];
  const pageSize = category.pageSize || PAGE_SIZE;
  const total = state.filtered.length;
  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  state.page = Math.min(state.page, pageCount - 1);
  const start = state.page * pageSize;
  const records = state.filtered.slice(start, start + pageSize);
  el("record-count").textContent = formatNumber(total);
  el("result-note").textContent = total ? `Hiển thị ${formatNumber(start + 1)}–${formatNumber(Math.min(start + pageSize, total))}` : "0 kết quả";
  grid.setAttribute("aria-busy", "false");

  if (!records.length) {
    grid.innerHTML = `<div class="empty-state"><div><span class="empty-code">0</span>
      <h2>Không tìm thấy mục phù hợp</h2><p>Thử từ khóa ngắn hơn hoặc xóa bộ lọc tìm kiếm.</p>
      <div class="empty-actions"><button class="button" type="button" data-clear-search>Xóa tìm kiếm</button></div>
    </div></div>`;
  } else if (category.layout === "table") {
    grid.innerHTML = renderTable(category, records, start);
  } else if (category.group) {
    grid.innerHTML = renderGroups(category, records, start);
  } else {
    grid.innerHTML = `<div class="card-grid">${records.map((record, index) => renderCard(category, record, start + index)).join("")}</div>`;
  }
  mountImageFallbacks(grid);
  renderPagination(pageCount);
}

function renderPagination(pageCount) {
  if (pageCount <= 1) {
    pagination.innerHTML = "";
    return;
  }
  const pages = new Set([0, pageCount - 1, state.page - 1, state.page, state.page + 1]);
  const visible = [...pages].filter((page) => page >= 0 && page < pageCount).sort((a, b) => a - b);
  let previous = -1;
  const middle = visible.map((page) => {
    const gap = previous >= 0 && page - previous > 1 ? `<span class="page-button" aria-hidden="true">…</span>` : "";
    previous = page;
    return `${gap}<button class="page-button" type="button" data-page="${page}" ${page === state.page ? 'aria-current="page"' : ""}>${page + 1}</button>`;
  }).join("");
  pagination.innerHTML = `
    <button class="page-button" type="button" data-page="${state.page - 1}" ${state.page === 0 ? "disabled" : ""}>← Trước</button>
    ${middle}
    <button class="page-button" type="button" data-page="${state.page + 1}" ${state.page === pageCount - 1 ? "disabled" : ""}>Sau →</button>`;
}

function formatValue(value) {
  if (value === null || value === undefined || value === "") return "—";
  if (typeof value === "boolean") return value ? "Có" : "Không";
  if (typeof value === "number") return formatNumber(value);
  if (typeof value === "object") return `<pre>${escapeHtml(JSON.stringify(value, null, 2))}</pre>`;
  return escapeHtml(value);
}

function openDetail(index, invoker = null) {
  const record = state.filtered[index];
  if (!record) return;
  state.selected = record;
  state.invoker = invoker;
  const category = CATEGORIES[state.category];
  const name = recordName(record);
  el("detail-kind").textContent = category.title;
  el("detail-title").textContent = name;
  const image = iconMarkup(category.image(record), name, true);
  const description = compactText(category.detail(record));
  const fields = Object.entries(record).map(([key, value]) => `
    <div class="field-row"><dt>${escapeHtml(FIELD_LABELS[key] || key)}</dt><dd>${formatValue(value)}</dd></div>`).join("");
  el("detail-body").innerHTML = `
    <div class="detail-hero">${image}<div><span class="id-chip">ID ${escapeHtml(recordId(record))}</span><p>${escapeHtml(description)}</p></div></div>
    <dl class="field-list">${fields}</dl>`;
  mountImageFallbacks(el("detail-body"));
  document.querySelectorAll("[data-index]").forEach((node) => node.setAttribute("aria-selected", String(Number(node.dataset.index) === index)));
  const panel = el("detail-panel");
  panel.inert = false;
  panel.classList.add("open");
  panel.setAttribute("aria-hidden", "false");
  el("detail-close").focus({ preventScroll: true });
}

function closeDetail(restoreFocus = true) {
  const panel = el("detail-panel");
  panel.classList.remove("open");
  panel.setAttribute("aria-hidden", "true");
  panel.inert = true;
  state.selected = null;
  document.querySelectorAll("[data-index]").forEach((node) => node.setAttribute("aria-selected", "false"));
  if (restoreFocus && state.invoker?.isConnected) state.invoker.focus({ preventScroll: true });
  state.invoker = null;
}

let toastTimer;
function showToast(message) {
  const toast = el("toast");
  toast.textContent = message;
  toast.classList.add("show");
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.classList.remove("show"), 1800);
}

async function copyText(text, message = "Đã chép") {
  try {
    await navigator.clipboard.writeText(String(text));
  } catch {
    const textarea = document.createElement("textarea");
    textarea.value = String(text);
    document.body.append(textarea);
    textarea.select();
    document.execCommand("copy");
    textarea.remove();
  }
  showToast(message);
}

async function loadFreshness() {
  const freshness = el("freshness");
  try {
    const response = await fetch(`${DATA_ROOT}/LastUpdated`, { cache: "no-store" });
    if (!response.ok) throw new Error(String(response.status));
    const text = (await response.text()).trim();
    const date = new Date(text);
    freshness.className = "freshness ready";
    freshness.querySelector("span:last-child").textContent = Number.isNaN(date.getTime())
      ? "Dữ liệu đã sẵn sàng"
      : `Cập nhật ${new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" }).format(date)}`;
  } catch {
    freshness.className = "freshness error";
    freshness.querySelector("span:last-child").textContent = "Đang chờ lượt dữ liệu đầu tiên";
  }
}

async function loadImageIndex() {
  try {
    const response = await fetch(`${DATA_ROOT}/Images.json`, { cache: "no-store" });
    if (response.ok) state.images = new Set((await response.json()).map(Number));
  } catch {
    state.images = null;
  }
}

function categoryFromHash() {
  const match = location.hash.match(/^#\/([a-z]+)/i);
  return match && CATEGORIES[match[1]] ? match[1] : "equipment";
}

routeContainer.addEventListener("click", (event) => {
  const button = event.target.closest("[data-category]");
  if (button) loadCategory(button.dataset.category);
});

searchInput.addEventListener("input", () => {
  state.query = searchInput.value;
  applyFilters(true);
});

sortSelect.addEventListener("change", () => {
  state.sort = sortSelect.value;
  applyFilters(true);
});

grid.addEventListener("click", (event) => {
  const copy = event.target.closest("[data-copy]");
  if (copy) {
    event.stopPropagation();
    copyText(copy.dataset.copy, `Đã chép ID ${copy.dataset.copy}`);
    return;
  }
  if (event.target.closest("[data-retry]")) {
    state.cache.delete(CATEGORIES[state.category].file);
    loadCategory(state.category, false);
    return;
  }
  if (event.target.closest("[data-clear-search]")) {
    searchInput.value = "";
    state.query = "";
    applyFilters(true);
    searchInput.focus();
    return;
  }
  const record = event.target.closest("[data-index]");
  if (record) openDetail(Number(record.dataset.index), record);
});

grid.addEventListener("keydown", (event) => {
  if (event.key !== "Enter" && event.key !== " ") return;
  const record = event.target.closest("[data-index]");
  if (!record || event.target.closest("[data-copy]")) return;
  event.preventDefault();
  openDetail(Number(record.dataset.index), record);
});

pagination.addEventListener("click", (event) => {
  const button = event.target.closest("[data-page]");
  if (!button || button.disabled) return;
  state.page = Number(button.dataset.page);
  renderRecords();
  el("records").scrollIntoView({ block: "start" });
});

el("detail-close").addEventListener("click", closeDetail);
el("copy-json").addEventListener("click", () => {
  if (state.selected) copyText(JSON.stringify(state.selected, null, 2), "Đã chép JSON");
});

document.addEventListener("keydown", (event) => {
  if (event.key === "/" && document.activeElement !== searchInput && !/INPUT|TEXTAREA|SELECT/.test(document.activeElement?.tagName)) {
    event.preventDefault();
    searchInput.focus();
  }
  if (event.key === "Escape" && el("detail-panel").classList.contains("open")) closeDetail();
});

window.addEventListener("hashchange", () => loadCategory(categoryFromHash(), false));

async function init() {
  renderRoutes();
  await Promise.allSettled([loadFreshness(), loadImageIndex()]);
  await loadCategory(categoryFromHash(), false);
}

init();
