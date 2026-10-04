import os
import re
import json

station_dir = r"h:\水晶光电AOI程序1002\Autoframe\Station"
output_file = r"h:\水晶光电AOI程序1002\vocabulary.json"

terms = set()

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

for filename in os.listdir(station_dir):
    if not filename.endswith(".cs"): continue
    filepath = os.path.join(station_dir, filename)
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        try: content = f.read()
        except UnicodeDecodeError:
            with open(filepath, 'r', encoding='gbk') as f2: content = f2.read()
            
    terms.add(filename.replace(".cs", ""))
            
    produce_body = extract_method(content, "ProduceRun")
    if not produce_body: continue
    
    # Extract Cases
    for m in re.finditer(r'case\s+"([^"]+)":', produce_body):
        terms.add(m.group(1))
    
    # Extract RegBit, DI, DO, AXIS
    for m in re.finditer(r'WaitRegBit\s*\(\s*RegBit\.([^,]+),', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'WaitDI\s*\(\s*DI\.([^,]+),', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'DO\.([^.]+)\.Set', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'AxisSingleTo\s*\([^,]+,\s*AXIS\.([^,]+)', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'WaitAxis\s*\(\s*AXIS\.([^,\)]+)', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'RegBit\.([^.]+)\.Set', produce_body): terms.add(m.group(1))
    for m in re.finditer(r'return\s+"([^"]+)"', produce_body): terms.add(m.group(1))

with open(output_file, 'w', encoding='utf-8') as f:
    json.dump(list(terms), f, ensure_ascii=False, indent=4)
