from pathlib import Path
import re, sys, json
from collections import defaultdict

root = Path(sys.argv[1] if len(sys.argv) > 1 else '.')
out = Path(sys.argv[2] if len(sys.argv) > 2 else 'audit/ui_visibility_audit.md')
console = root / 'Project Files/Source/Console'
designer = console / 'setup.designer.cs'
setup = console / 'setup.cs'

D = designer.read_text(encoding='utf-8-sig', errors='replace')
S = setup.read_text(encoding='utf-8-sig', errors='replace')
all_cs = ''
for p in console.glob('*.cs'):
    try: all_cs += '\n//FILE:'+p.name+'\n'+p.read_text(encoding='utf-8-sig', errors='replace')
    except Exception: pass

# Parse designer properties.
loc = {m.group(1):(int(m.group(2)),int(m.group(3))) for m in re.finditer(r'this\.(\w+)\.Location\s*=\s*new System\.Drawing\.Point\((-?\d+),\s*(-?\d+)\);', D)}
size = {m.group(1):(int(m.group(2)),int(m.group(3))) for m in re.finditer(r'this\.(\w+)\.Size\s*=\s*new System\.Drawing\.Size\((\d+),\s*(\d+)\);', D)}
text = {m.group(1):m.group(2) for m in re.finditer(r'this\.(\w+)\.Text\s*=\s*"([^"]*)";', D)}
visible_false = set(m.group(1) for m in re.finditer(r'this\.(\w+)\.Visible\s*=\s*false\s*;', D))
enabled_false = set(m.group(1) for m in re.finditer(r'this\.(\w+)\.Enabled\s*=\s*false\s*;', D))
autoscroll = set(m.group(1) for m in re.finditer(r'this\.(\w+)\.AutoScroll\s*=\s*true\s*;', D))

parent = {}
# Parent relationships from parent.Controls.Add(child)
for m in re.finditer(r'this\.(\w+)\.Controls\.Add\(this\.(\w+)\);', D):
    parent[m.group(2)] = m.group(1)
# top-level form controls
for m in re.finditer(r'this\.Controls\.Add\(this\.(\w+)\);', D):
    parent.setdefault(m.group(1), '<FORM>')

# All declared control-like fields (designer declarations near end).
declared = set(m.group(1) for m in re.finditer(r'private\s+(?:System\.Windows\.Forms\.)?[\w\.<>]+\s+(\w+)\s*;', D))

# Geometry clipping flags.
clip = []
for c,p in parent.items():
    if c not in loc or c not in size or p not in size: continue
    x,y=loc[c]; w,h=size[c]; pw,ph=size[p]
    over = []
    if x < 0: over.append(f'left={x}')
    if y < 0: over.append(f'top={y}')
    if x+w > pw: over.append(f'right={x+w}>{pw}')
    if y+h > ph: over.append(f'bottom={y+h}>{ph}')
    if over and p not in autoscroll:
        clip.append((c,p,loc[c],size[c],size[p],', '.join(over)))

zero = [(c,loc.get(c),size[c]) for c in size if size[c][0] == 0 or size[c][1] == 0]
negative = [(c,loc[c],size.get(c)) for c in loc if loc[c][0] < 0 or loc[c][1] < 0]

# Orphan controls: declared + sized/located but no known parent. Exclude helper non-controls by requiring geometry.
orphan = sorted(c for c in declared if c in size and c in loc and c not in parent)

# Tab pages declared but not added to any control.
tab_like = sorted(c for c in declared if ('tab' in c.lower() or 'page' in c.lower()) and c in size and c not in parent)

# Hidden controls and evidence of runtime toggling.
hidden_rows=[]
for c in sorted(visible_false):
    patterns = [
        rf'\b{re.escape(c)}\.Visible\s*=\s*true',
        rf'\b{re.escape(c)}\.Show\s*\(',
        rf'\b{re.escape(c)}\.Visible\s*=\s*[^f][^;]*;'
    ]
    toggled = any(re.search(p, all_cs) for p in patterns)
    hidden_rows.append((c,parent.get(c,'?'),text.get(c,''),toggled))

# Disabled controls and runtime enabling evidence.
disabled_rows=[]
for c in sorted(enabled_false):
    toggled = bool(re.search(rf'\b{re.escape(c)}\.Enabled\s*=\s*true', all_cs))
    disabled_rows.append((c,parent.get(c,'?'),text.get(c,''),toggled))

# Dynamic controls in setup.cs.
dyn_new = []
for m in re.finditer(r'(?m)^\s*(?:private\s+)?(?:[\w<>]+\s+)?(\w+)\s*=\s*new\s+(\w+(?:TS)?)\s*\(\s*\)\s*;', S):
    var, typ = m.group(1), m.group(2)
    if typ.lower().endswith(('ts','box','label','button','control','panel','page','bar')) or typ in ['CheckBox','ComboBox','TrackBar','GroupBox','Label','Button','Panel']:
        line = S.count('\n',0,m.start())+1
        dyn_new.append((var,typ,line))

# Dynamic UI init methods: methods containing new UI controls.
method_re = re.compile(r'(?m)^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:void|\w+)\s+(\w+)\s*\([^)]*\)\s*\{')
dyn_methods=[]
for mm in method_re.finditer(S):
    name=mm.group(1); brace=S.find('{',mm.start()); level=0; end=None
    for i in range(brace,len(S)):
        if S[i]=='{': level+=1
        elif S[i]=='}':
            level-=1
            if level==0: end=i+1; break
    if end is None: continue
    body=S[brace:end]
    news=re.findall(r'new\s+(CheckBoxTS|ComboBoxTS|TrackBarTS|GroupBoxTS|LabelTS|ButtonTS|Panel|TabPageTS|NumericUpDownTS)\s*\(',body)
    if news:
        line=S.count('\n',0,mm.start())+1
        calls = len(re.findall(r'\b'+re.escape(name)+r'\s*\(', S)) - 1
        dyn_methods.append((name,line,len(news),calls))

# EU2AV/SQ4KOU UI markers near dynamic methods / declarations.
markers=[]
for i,line in enumerate(S.splitlines(),1):
    if ('Yurij' in line or 'eu2av' in line.lower() or 'SQ4KOU' in line) and any(k in line.lower() for k in ['control','checkbox','button','setup','display','phase','calib','waterfall','n1mm','dpi','visible']):
        markers.append((i,line.strip()))

# Programmatic parent/placement statements in known enhancement methods, contextual extraction.
focus_names=['CreateDpiAwarenessCheckBox','InitWaterfallQualityControls','InitPhaseRotatorControls','InitDetCalTab','initVoltsAmpsCalibration','InitN1mmCWShiftOption']
focus=[]
for name in focus_names:
    m=re.search(r'(?m)^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:void|\w+)\s+'+re.escape(name)+r'\s*\([^)]*\)\s*\{',S)
    if not m: continue
    brace=S.find('{',m.start()); level=0; end=None
    for i in range(brace,len(S)):
        if S[i]=='{': level+=1
        elif S[i]=='}':
            level-=1
            if level==0: end=i+1; break
    body=S[m.start():end]
    line=S.count('\n',0,m.start())+1
    snippets=[]
    for bl in body.splitlines():
        if any(k in bl for k in ['Parent','Controls.Add','Location =','Size =','Visible =','Enabled =','BringToFront','Text =']):
            snippets.append(bl.strip())
    focus.append((name,line,snippets[:40]))

# Overlap of sibling GROUPS/PANELS only, to avoid intentional label/control overlaps.
groups=[c for c in loc if c in size and (c.lower().startswith(('grp','groupbox','panel')))]
overlap=[]
bypar=defaultdict(list)
for c in groups:
    if c in parent: bypar[parent[c]].append(c)
for p,cs in bypar.items():
    for i,a in enumerate(cs):
        ax,ay=loc[a]; aw,ah=size[a]
        for b in cs[i+1:]:
            bx,by=loc[b]; bw,bh=size[b]
            ix=max(0,min(ax+aw,bx+bw)-max(ax,bx)); iy=max(0,min(ay+ah,by+bh)-max(ay,by))
            area=ix*iy
            if area>0:
                frac=area/min(max(1,aw*ah),max(1,bw*bh))
                if frac>=0.15:
                    overlap.append((a,b,p,area,frac))

# Classify high-value findings.
# Ignore common intentional controls based on naming; report raw and concise shortlist.
high=[]
for row in clip:
    c,p,cl,cs,ps,why=row
    # buttons with only 1-2 px can be DPI rounding; require >3px overflow for high
    x,y=cl; w,h=cs; pw,ph=ps
    overflow=max(max(0,-x),max(0,-y),max(0,x+w-pw),max(0,y+h-ph))
    if overflow>=4: high.append(('CLIP',c,p,why))
for c,p,t,toggled in hidden_rows:
    if not toggled: high.append(('HIDDEN_STATIC',c,p,t or 'Visible=false, no explicit show found'))
for c in orphan[:]:
    high.append(('ORPHAN',c,'?', 'has geometry but no Controls.Add parent found'))

lines=[]
lines += ['# Setup UI visibility audit','',f'- Branch audited: `{root}` (working checkout)',f'- Designer controls with geometry: **{len(size)}**',f'- Known parent links: **{len(parent)}**',f'- Geometric clipping candidates: **{len(clip)}**',f'- `Visible=false` controls: **{len(hidden_rows)}**',f'- `Enabled=false` controls: **{len(disabled_rows)}**',f'- Dynamic UI creation sites in `setup.cs`: **{len(dyn_new)}**',f'- Dynamic UI methods: **{len(dyn_methods)}**',f'- Orphan geometry candidates: **{len(orphan)}**','']

lines += ['## Priority findings','']
if high:
    lines += ['| Type | Control | Parent | Reason |','|---|---|---|---|']
    for typ,c,p,why in high[:120]: lines.append(f'| {typ} | `{c}` | `{p}` | {why.replace("|","/")} |')
else: lines.append('No high-priority static visibility failures detected.')
lines.append('')

lines += ['## Geometric clipping candidates','']
if clip:
    lines += ['| Control | Parent | Loc | Size | Parent size | Overflow |','|---|---|---:|---:|---:|---|']
    for c,p,l,s,ps,w in clip: lines.append(f'| `{c}` | `{p}` | `{l}` | `{s}` | `{ps}` | {w} |')
else: lines.append('None.')
lines.append('')

lines += ['## Controls explicitly hidden in designer','']
if hidden_rows:
    lines += ['| Control | Parent | Caption | Runtime show/toggle found |','|---|---|---|---|']
    for c,p,t,tog in hidden_rows: lines.append(f'| `{c}` | `{p}` | {t.replace("|","/")} | {"YES" if tog else "NO"} |')
else: lines.append('None.')
lines.append('')

lines += ['## Controls explicitly disabled in designer','']
if disabled_rows:
    lines += ['| Control | Parent | Caption | Runtime enable found |','|---|---|---|---|']
    for c,p,t,tog in disabled_rows: lines.append(f'| `{c}` | `{p}` | {t.replace("|","/")} | {"YES" if tog else "NO"} |')
else: lines.append('None.')
lines.append('')

lines += ['## Dynamic UI methods','', '| Method | Line | UI objects created | Other call sites |','|---|---:|---:|---:|']
for n,l,k,calls in dyn_methods: lines.append(f'| `{n}` | {l} | {k} | {calls} |')
lines.append('')

lines += ['## Focus: post-upstream / programmatic settings','']
for n,l,snips in focus:
    lines += [f'### `{n}` (line {l})','```']+snips+['```','']

lines += ['## Significant sibling group overlaps','']
if overlap:
    lines += ['| A | B | Parent | Overlap fraction of smaller |','|---|---|---|---:|']
    for a,b,p,area,frac in overlap: lines.append(f'| `{a}` | `{b}` | `{p}` | {frac:.1%} |')
else: lines.append('None >= 15%.')
lines.append('')

lines += ['## Orphan geometry candidates','']
if orphan: lines += [', '.join(f'`{x}`' for x in orphan)]
else: lines.append('None.')
lines.append('')

lines += ['## EU2AV / SQ4KOU UI markers','']
for l,s in markers[:200]: lines.append(f'- L{l}: `{s.replace("`","\'")}`')

out.parent.mkdir(parents=True, exist_ok=True)
out.write_text('\n'.join(lines), encoding='utf-8')
print('\n'.join(lines[:80]))
print(f'REPORT={out}')
