from pathlib import Path
import re, sys
from collections import defaultdict

root = Path(sys.argv[1] if len(sys.argv) > 1 else '.')
out = Path(sys.argv[2] if len(sys.argv) > 2 else 'audit/ui_visibility_audit.md')
console = root / 'Project Files/Source/Console'
designer = console / 'setup.designer.cs'
setup = console / 'setup.cs'

D = designer.read_text(encoding='utf-8-sig', errors='replace')
S = setup.read_text(encoding='utf-8-sig', errors='replace')
all_cs_parts=[]
for p in console.glob('*.cs'):
    try: all_cs_parts.append(p.read_text(encoding='utf-8-sig', errors='replace'))
    except Exception: pass
all_cs='\n'.join(all_cs_parts)

loc={m.group(1):(int(m.group(2)),int(m.group(3))) for m in re.finditer(r'this\.(\w+)\.Location\s*=\s*new System\.Drawing\.Point\((-?\d+),\s*(-?\d+)\);',D)}
size={m.group(1):(int(m.group(2)),int(m.group(3))) for m in re.finditer(r'this\.(\w+)\.Size\s*=\s*new System\.Drawing\.Size\((\d+),\s*(\d+)\);',D)}
caption={m.group(1):m.group(2) for m in re.finditer(r'this\.(\w+)\.Text\s*=\s*"([^"]*)";',D)}
visible_false=set(m.group(1) for m in re.finditer(r'this\.(\w+)\.Visible\s*=\s*false\s*;',D))
enabled_false=set(m.group(1) for m in re.finditer(r'this\.(\w+)\.Enabled\s*=\s*false\s*;',D))
autoscroll=set(m.group(1) for m in re.finditer(r'this\.(\w+)\.AutoScroll\s*=\s*true\s*;',D))
parent={}
for m in re.finditer(r'this\.(\w+)\.Controls\.Add\(this\.(\w+)\);',D): parent[m.group(2)]=m.group(1)
for m in re.finditer(r'this\.Controls\.Add\(this\.(\w+)\);',D): parent.setdefault(m.group(1),'<FORM>')
declared=set(m.group(1) for m in re.finditer(r'private\s+(?:System\.Windows\.Forms\.)?[\w\.<>]+\s+(\w+)\s*;',D))

clip=[]
for c,p in parent.items():
    if c not in loc or c not in size or p not in size: continue
    x,y=loc[c]; w,h=size[c]; pw,ph=size[p]; over=[]
    if x<0: over.append(f'left={x}')
    if y<0: over.append(f'top={y}')
    if x+w>pw: over.append(f'right={x+w}>{pw}')
    if y+h>ph: over.append(f'bottom={y+h}>{ph}')
    if over and p not in autoscroll: clip.append((c,p,loc[c],size[c],size[p],', '.join(over)))

orphan=sorted(c for c in declared if c in size and c in loc and c not in parent)
hidden=[]
for c in sorted(visible_false):
    toggled=bool(re.search(rf'\b{re.escape(c)}\.(?:Visible\s*=\s*true|Show\s*\()',all_cs))
    hidden.append((c,parent.get(c,'?'),caption.get(c,''),toggled))
disabled=[]
for c in sorted(enabled_false):
    toggled=bool(re.search(rf'\b{re.escape(c)}\.Enabled\s*=\s*true',all_cs))
    disabled.append((c,parent.get(c,'?'),caption.get(c,''),toggled))

# Method ranges approximated by start-to-next-method; fast and sufficient for UI creation inventory.
method_re=re.compile(r'(?m)^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:void|[\w<>\[\],]+)\s+(\w+)\s*\([^;{}]*\)\s*\{')
methods=list(method_re.finditer(S))
dyn_methods=[]
ui_new_re=re.compile(r'new\s+(CheckBoxTS|ComboBoxTS|TrackBarTS|GroupBoxTS|LabelTS|ButtonTS|Panel|TabPageTS|NumericUpDownTS)\s*\(')
for idx,m in enumerate(methods):
    end=methods[idx+1].start() if idx+1<len(methods) else len(S)
    body=S[m.start():end]
    news=ui_new_re.findall(body)
    if news:
        name=m.group(1); line=S.count('\n',0,m.start())+1
        calls=max(0,len(re.findall(r'\b'+re.escape(name)+r'\s*\(',S))-1)
        dyn_methods.append((name,line,len(news),calls))

# Exact block extractor only for a handful of targeted methods.
def block_for(name):
    m=re.search(r'(?m)^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:void|[\w<>\[\],]+)\s+'+re.escape(name)+r'\s*\([^;{}]*\)\s*\{',S)
    if not m: return None,None
    brace=S.find('{',m.start()); level=0
    for i in range(brace,len(S)):
        if S[i]=='{': level+=1
        elif S[i]=='}':
            level-=1
            if level==0: return S[m.start():i+1], S.count('\n',0,m.start())+1
    return None,None

focus_names=['CreateDpiAwarenessCheckBox','InitWaterfallQualityControls','InitPhaseRotatorControls','InitDetCalTab','initVoltsAmpsCalibration','InitN1mmCWShiftOption']
focus=[]
for name in focus_names:
    body,line=block_for(name)
    if not body: continue
    snippets=[]
    for bl in body.splitlines():
        if any(k in bl for k in ['Parent','Controls.Add','Location =','Size =','Visible =','Enabled =','BringToFront','Text =']): snippets.append(bl.strip())
    focus.append((name,line,snippets[:60]))

markers=[]
for i,line in enumerate(S.splitlines(),1):
    low=line.lower()
    if ('yurij' in low or 'eu2av' in low or 'sq4kou' in low) and any(k in low for k in ['control','checkbox','button','setup','display','phase','calib','waterfall','n1mm','dpi','visible']): markers.append((i,line.strip()))

groups=[c for c in loc if c in size and c.lower().startswith(('grp','groupbox','panel'))]
bypar=defaultdict(list)
for c in groups:
    if c in parent: bypar[parent[c]].append(c)
overlap=[]
for p,cs in bypar.items():
    for i,a in enumerate(cs):
        ax,ay=loc[a]; aw,ah=size[a]
        for b in cs[i+1:]:
            bx,by=loc[b]; bw,bh=size[b]
            ix=max(0,min(ax+aw,bx+bw)-max(ax,bx)); iy=max(0,min(ay+ah,by+bh)-max(ay,by)); area=ix*iy
            if area:
                frac=area/min(max(1,aw*ah),max(1,bw*bh))
                if frac>=0.15: overlap.append((a,b,p,frac))

high=[]
for c,p,cl,cs,ps,why in clip:
    x,y=cl; w,h=cs; pw,ph=ps
    overflow=max(max(0,-x),max(0,-y),max(0,x+w-pw),max(0,y+h-ph))
    if overflow>=4: high.append(('CLIP',c,p,why))
for c,p,t,toggled in hidden:
    if not toggled: high.append(('HIDDEN_STATIC',c,p,t or 'Visible=false, no explicit show found'))
for c in orphan: high.append(('ORPHAN',c,'?','geometry exists but no Controls.Add parent found'))

lines=['# Setup UI visibility audit','',f'- Designer controls with geometry: **{len(size)}**',f'- Known parent links: **{len(parent)}**',f'- Geometric clipping candidates: **{len(clip)}**',f'- `Visible=false` controls: **{len(hidden)}**',f'- `Enabled=false` controls: **{len(disabled)}**',f'- Dynamic UI methods: **{len(dyn_methods)}**',f'- Orphan geometry candidates: **{len(orphan)}**','']
lines += ['## Priority findings','']
if high:
    lines += ['| Type | Control | Parent | Reason |','|---|---|---|---|']
    for typ,c,p,why in high[:160]: lines.append(f'| {typ} | `{c}` | `{p}` | {why.replace("|","/")} |')
else: lines.append('No high-priority static visibility failures detected.')
lines.append('')

lines += ['## Geometric clipping candidates','']
if clip:
    lines += ['| Control | Parent | Loc | Size | Parent size | Overflow |','|---|---|---:|---:|---:|---|']
    for c,p,l,s,ps,w in clip: lines.append(f'| `{c}` | `{p}` | `{l}` | `{s}` | `{ps}` | {w} |')
else: lines.append('None.')
lines.append('')

lines += ['## Controls explicitly hidden in designer','']
if hidden:
    lines += ['| Control | Parent | Caption | Runtime show found |','|---|---|---|---|']
    for c,p,t,tog in hidden: lines.append(f'| `{c}` | `{p}` | {t.replace("|","/")} | {"YES" if tog else "NO"} |')
else: lines.append('None.')
lines.append('')

lines += ['## Controls explicitly disabled in designer','']
if disabled:
    lines += ['| Control | Parent | Caption | Runtime enable found |','|---|---|---|---|']
    for c,p,t,tog in disabled: lines.append(f'| `{c}` | `{p}` | {t.replace("|","/")} | {"YES" if tog else "NO"} |')
else: lines.append('None.')
lines.append('')

lines += ['## Dynamic UI methods','', '| Method | Line | UI objects created | Other call sites |','|---|---:|---:|---:|']
for n,l,k,calls in dyn_methods: lines.append(f'| `{n}` | {l} | {k} | {calls} |')
lines.append('')

lines += ['## Focus: programmatic settings added around EU2AV/SQ4KOU work','']
for n,l,snips in focus:
    lines += [f'### `{n}` (line {l})','```']+snips+['```','']

lines += ['## Significant sibling group overlaps','']
if overlap:
    lines += ['| A | B | Parent | Smaller-group overlap |','|---|---|---|---:|']
    for a,b,p,frac in overlap: lines.append(f'| `{a}` | `{b}` | `{p}` | {frac:.1%} |')
else: lines.append('None >= 15%.')
lines.append('')

lines += ['## Orphan geometry candidates','']
lines.append(', '.join(f'`{x}`' for x in orphan) if orphan else 'None.')
lines += ['','## EU2AV / SQ4KOU UI markers','']
for l,s in markers[:240]:
    safe=s.replace('`',"'")
    lines.append(f'- L{l}: `{safe}`')

out.parent.mkdir(parents=True,exist_ok=True)
out.write_text('\n'.join(lines),encoding='utf-8')
print('\n'.join(lines[:100]))
print(f'REPORT={out}')
