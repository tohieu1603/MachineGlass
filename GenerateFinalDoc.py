import os
import re
from docx import Document
from docx.shared import Pt, RGBColor, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement

station_dir = r"h:\水晶光电AOI程序1002\Autoframe\Station"
output_file = r"h:\水晶光电AOI程序1002\AOI_Project_Full_Workflow.docx"

word_dict = {
    "上料站": "Trạm Cấp Liệu", "拉料站": "Trạm Kéo Liệu", "扩膜站": "Trạm Căng Màng",
    "摆臂站": "Trạm Tay Gắp", "移载": "Trạm Chuyển Tải", "翻转": "Trạm Lật",
    "AR检测1": "Trạm Kiểm Tra AR 1", "AR检测2": "Trạm Kiểm Tra AR 2",
    "IR检测1": "Trạm Kiểm Tra IR 1", "IR检测2": "Trạm Kiểm Tra IR 2",
    "OK覆膜站": "Trạm Dán Màng OK", "NG覆膜站": "Trạm Dán Màng NG",
    "OK下料站": "Trạm Xuất Liệu OK", "NG下料站": "Trạm Xuất Liệu NG",
    "OK料仓站": "Kho Chứa OK", "NG料仓站": "Kho Chứa NG",
    "StationRobot": "Robot Trung Tâm", "DataUpdata": "Trạm Cập Nhật Dữ Liệu",
    
    "初始化": "Khởi tạo", "生产流程开始": "Bắt đầu sản xuất", "生产流程结束": "Kết thúc sản xuất",
    
    "运动到": "Di chuyển tới", "移动到": "Di chuyển tới", "到": "Tới", "回": "Về",
    "等待": "Chờ", "完成": "hoàn tất", "换料": "đổi vật liệu", "上料": "cấp liệu",
    "下料": "xuất liệu", "取料": "lấy đồ", "放料": "thả đồ", "拍照": "chụp ảnh",
    "破真空": "Tắt hút chân không", "吸真空": "Bật hút chân không",
    "检查": "Kiểm tra", "通知": "Báo cáo", "打开": "Mở", "关闭": "Đóng", 
    "有料": "Có vật", "准备": "chuẩn bị", "下降": "hạ xuống", "上升": "nâng lên",
    
    "轴": "Trục ", "气缸": "Xi-lanh ", "升降": "Nâng hạ ", "吸嘴": "Vòi hút ",
    "钢环": "Vòng thép", "料盘": "Khay liệu", "顶针": "Kim đẩy", "流线": "Băng tải",
    
    "待机位": "Vị trí chờ", "上料位": "Vị trí cấp liệu", "取料位": "Vị trí lấy liệu",
    "放料位": "Vị trí thả liệu", "拍照位": "Vị trí chụp ảnh", "换料位": "Vị trí đổi liệu"
}

def translate_zh_vi(text):
    if not text: return text
    clean_text = text.replace('RegBit.', '').replace('DI.', '').replace('DO.', '').replace('AXIS.', '')
    res = clean_text
    sorted_keys = sorted(word_dict.keys(), key=len, reverse=True)
    for zh in sorted_keys:
        res = res.replace(zh, word_dict[zh] + " ")
    res = " ".join(res.split())
    return res.strip() if res != clean_text else clean_text

def extract_method(content, method_name):
    pattern = r'protected\s+override\s+string\s+' + method_name + r'\s*\(\s*string\s+strStep\s*\)\s*\{'
    match = re.search(pattern, content)
    if not match: return None
    start_idx = match.end()
    open_brackets = 1
    end_idx = start_idx
    for i, char in enumerate(content[start_idx:], start_idx):
        if char == '{': open_brackets += 1
        elif char == '}':
            open_brackets -= 1
            if open_brackets == 0:
                end_idx = i
                break
    return content[start_idx:end_idx]

def extract_cases(method_body):
    cases = []
    case_pattern = re.compile(r'case\s+"([^"]+)":')
    matches = list(case_pattern.finditer(method_body))
    for i in range(len(matches)):
        case_name = matches[i].group(1)
        start_idx = matches[i].end()
        if i < len(matches) - 1:
            end_idx = matches[i+1].start()
        else:
            default_match = re.search(r'\bdefault\s*:', method_body[start_idx:])
            end_idx = start_idx + default_match.start() if default_match else len(method_body)
        cases.append({"step": case_name, "code": method_body[start_idx:end_idx].strip()})
    return cases

def parse_semantic_actions(code):
    actions = []
    for m in re.finditer(r'WaitRegBit\s*\(\s*RegBit\.([^,]+),\s*([^,\)]+)', code):
        actions.append(f"Chờ tín hiệu hệ thống [{translate_zh_vi(m.group(1))}] == {m.group(2).strip()}")
    for m in re.finditer(r'WaitDI\s*\(\s*DI\.([^,]+),\s*([^,\)]+)', code):
        actions.append(f"Chờ cảm biến [{translate_zh_vi(m.group(1))}] == {m.group(2).strip()}")
    for m in re.finditer(r'DO\.([^.]+)\.Set\s*\(\s*([^)]+)\)', code):
        state = "BẬT" if "true" in m.group(2).strip().lower() else "TẮT"
        actions.append(f"Điều khiển I/O: {state} [{translate_zh_vi(m.group(1))}]")
    for m in re.finditer(r'AxisSingleTo\s*\([^,]+,\s*AXIS\.([^,]+)', code):
        actions.append(f"Khởi động trục [{translate_zh_vi(m.group(1))}]")
    for m in re.finditer(r'WaitAxis\s*\(\s*AXIS\.([^,\)]+)', code):
        actions.append(f"Chờ trục [{translate_zh_vi(m.group(1))}] tới đích")
    for m in re.finditer(r'RegBit\.([^.]+)\.Set\s*\(\s*([^)]+)\)', code):
        state = "BẬT" if "true" in m.group(2).strip().lower() else "TẮT"
        actions.append(f"Báo cáo tín hiệu: {state} [{translate_zh_vi(m.group(1))}]")
    for m in re.finditer(r'return\s+"([^"]+)"', code):
        actions.append(f"Chuyển bước: [{translate_zh_vi(m.group(1))}]")
    return actions

def set_cell_background(cell, fill, color=None):
    shading_elm = OxmlElement('w:shd')
    shading_elm.set(qn('w:fill'), fill)
    cell._tc.get_or_add_tcPr().append(shading_elm)

# --- CREATE DOCUMENT ---
doc = Document()
style = doc.styles['Normal']
style.font.name = 'Arial'
style.font.size = Pt(11)

# Title
title = doc.add_heading('TÀI LIỆU PHÂN TÍCH LƯU TRÌNH DỰ ÁN AOI 1002', 0)
title.alignment = WD_ALIGN_PARAGRAPH.CENTER
doc.add_paragraph('\n')

# ----------------- PART 1: OVERALL WORKFLOW -----------------
doc.add_heading('PHẦN 1: TỔNG QUAN LUỒNG HOẠT ĐỘNG (OVERALL WORKFLOW)', level=1)
doc.add_paragraph(
    "Dự án này là hệ thống máy kiểm tra quang học tự động (AOI). Thiết kế phần mềm chia máy thành các trạm (Station) độc lập, "
    "giao tiếp với nhau qua các biến trạng thái (RegBit) theo mô hình Máy Trạng Thái (State Machine). Các trạm hoạt động song song "
    "và đồng bộ theo luồng vật lý chung như sau:"
)
flow_desc = (
    "1. Cấp liệu (上料/拉料): Hệ thống lấy vòng thép/khay từ kho (Silo) đẩy vào trong máy.\n"
    "2. Căng màng (扩膜): Xử lý kéo căng màng phim chứa sản phẩm.\n"
    "3. Tay gắp & Chuyển tải (摆臂/移载/翻转): Robot tay gắp hoặc cơ cấu chuyển tiếp sẽ hút sản phẩm, lật mặt và đưa vào khu vực kiểm tra.\n"
    "4. Kiểm tra Quang học (AR/IR 检测): Camera chụp và thuật toán AOI xử lý bề mặt, lưu kết quả (DataUpdate).\n"
    "5. Phân loại & Đóng màng (OK/NG 覆膜): Dựa vào kết quả kiểm tra, dán màng bảo vệ tương ứng.\n"
    "6. Ra liệu (下料/料仓): Đẩy sản phẩm đã kiểm tra xong ra khay OK hoặc khay NG."
)
for line in flow_desc.split('\n'):
    doc.add_paragraph(line, style='List Number')

doc.add_page_break()

# ----------------- PART 2: GLOSSARY TABLE -----------------
doc.add_heading('PHẦN 2: BẢNG TỪ VỰNG DỊCH THUẬT QUY ƯỚC', level=1)
doc.add_paragraph("Bảng dưới đây liệt kê các thuật ngữ tiếng Trung cốt lõi trong code và nghĩa tiếng Việt tương ứng được dùng trong tài liệu này.")

table_dict = doc.add_table(rows=1, cols=2)
table_dict.style = 'Table Grid'
hdr_cells = table_dict.rows[0].cells
hdr_cells[0].text = 'Thuật ngữ Gốc (Tiếng Trung)'
hdr_cells[1].text = 'Ý nghĩa (Tiếng Việt)'
set_cell_background(hdr_cells[0], "D9D9D9")
set_cell_background(hdr_cells[1], "D9D9D9")

for zh, vi in list(word_dict.items())[:20]: # Add top 20 terms
    row_cells = table_dict.add_row().cells
    row_cells[0].text = zh
    row_cells[1].text = vi

doc.add_page_break()

# ----------------- PART 3: DETAILED STATIONS -----------------
doc.add_heading('PHẦN 3: CHI TIẾT LƯU TRÌNH TỪNG TRẠM', level=1)

for filename in os.listdir(station_dir):
    if not filename.endswith(".cs"): continue
    filepath = os.path.join(station_dir, filename)
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        try: content = f.read()
        except UnicodeDecodeError:
            with open(filepath, 'r', encoding='gbk') as f2: content = f2.read()
            
    produce_body = extract_method(content, "ProduceRun")
    if not produce_body: continue
    
    st_name_zh = filename.replace(".cs", "")
    st_name_vi = translate_zh_vi(st_name_zh)
    
    doc.add_heading(f'Trạm: {st_name_vi} ({st_name_zh})', level=2)
    
    cases = extract_cases(produce_body)
    
    # Create Table
    doc.add_heading('3.1 Bảng Diễn Giải Logic (ProduceRun)', level=3)
    table = doc.add_table(rows=1, cols=3)
    table.style = 'Table Grid'
    hdr = table.rows[0].cells
    hdr[0].text = 'Tên Bước'
    hdr[1].text = 'Hành Động / Chờ Đợi'
    hdr[2].text = 'Bước Tiếp Theo'
    for c in hdr: set_cell_background(c, "D9E2F3")
    
    code_snippets = []
    
    for c in cases:
        step_zh = c['step']
        step_vi = translate_zh_vi(step_zh)
        actions = parse_semantic_actions(c['code'])
        
        # Filter actions for the table
        next_steps = [a.replace("Chuyển bước: ", "").replace("Điều hướng: ", "") for a in actions if "Chuyển bước" in a]
        logic_actions = [a for a in actions if "Chuyển bước" not in a]
        
        row = table.add_row().cells
        row[0].text = f"{step_vi}\n({step_zh})"
        row[1].text = "\n".join(logic_actions) if logic_actions else "(Xử lý nội bộ)"
        row[2].text = "\n".join(next_steps) if next_steps else "(Kết thúc/Ngầm định)"
        
        code_snippets.append((step_zh, c['code']))
        
    doc.add_paragraph("\n")
    doc.add_heading('3.2 Mã Nguồn Dẫn Chứng (Code Snippets)', level=3)
    
    for step_zh, code in code_snippets:
        p_name = doc.add_paragraph()
        p_name.add_run(f"Mã nguồn cho bước: \"{step_zh}\"").bold = True
        
        p_code = doc.add_paragraph()
        run = p_code.add_run(f"case \"{step_zh}\":\n" + code)
        run.font.name = 'Consolas'
        run.font.size = Pt(9)
        # Add border/shading to code block via paragraph format (simple indent)
        p_code.paragraph_format.left_indent = Inches(0.5)

    doc.add_page_break()

doc.save(output_file)
print(f"Successfully created {output_file}")
