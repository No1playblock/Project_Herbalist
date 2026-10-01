"""Trace the supplied maze images into saved Unity graph authoring data.
Requires PyMuPDF, Pillow, NumPy, SciPy. No runtime dependency.
Usage: python Tools/trace_stage_four_mazes.py path/to/design.pdf
"""
import argparse, json
from pathlib import Path
import fitz
import numpy as np
from PIL import Image
from scipy import ndimage as ndi
import io

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/_Project/Data/Stages/Stage_04/Layouts'
ART=ROOT/'Assets/_Project/Art/Environments/Stage_04/Textures'

def thin(image):
    a=np.pad(image.astype(bool),1)
    for iteration in range(300):
        count=0
        for phase in (0,1):
            p=[a[:-2,1:-1],a[:-2,2:],a[1:-1,2:],a[2:,2:],a[2:,1:-1],a[2:,:-2],a[1:-1,:-2],a[:-2,:-2]]
            n=sum(x.astype(np.uint8) for x in p)
            t=sum((~p[i]&p[(i+1)%8]).astype(np.uint8) for i in range(8))
            if phase==0: rule=(~(p[0]&p[2]&p[4])) & (~(p[2]&p[4]&p[6]))
            else: rule=(~(p[0]&p[2]&p[6])) & (~(p[0]&p[4]&p[6]))
            remove=a[1:-1,1:-1]&(n>=2)&(n<=6)&(t==1)&rule
            count+=int(remove.sum());a[1:-1,1:-1][remove]=False
        if not count: return a[1:-1,1:-1]
    raise RuntimeError('Thinning did not converge')

def simplify(points,tolerance=1.5):
    if len(points)<3:return [points[0],points[-1]]
    p=np.asarray(points,dtype=float);a=p[0];b=p[-1];v=b-a
    t=np.clip((p-a)@v/max(float(v@v),1),0,1)
    dist=np.linalg.norm(p-(a+t[:,None]*v),axis=1);i=int(dist.argmax())
    if dist[i]<=tolerance:return [points[0],points[-1]]
    return simplify(points[:i+1],tolerance)[:-1]+simplify(points[i:],tolerance)

def component_centers(mask,minsize):
    lab,num=ndi.label(mask)
    result=[]
    for i in range(1,num+1):
        yy,xx=np.where(lab==i)
        if len(xx)>=minsize:result.append((len(xx),(float(xx.mean()),float(yy.mean()))))
    return [p for _,p in sorted(result,reverse=True)]

def orthogonal_graph(points,edges,walls):
    def clear(a,b):
        count=int(np.linalg.norm(np.asarray(b)-a)*2)+2
        sample=np.rint(np.linspace(a,b,count)).astype(int)
        return not walls[sample[:,1],sample[:,0]].any()
    def route(a,b,depth=0):
        if a==b:return [a]
        if a[0]==b[0] or a[1]==b[1]:return [a,b]
        corners=[(b[0],a[1]),(a[0],b[1])]
        if abs(b[1]-a[1])>abs(b[0]-a[0]):corners.reverse()
        for c in corners:
            if clear(a,c) and clear(c,b):return [a,c,b]
        assert depth<15,('Cannot make cardinal passage',a,b)
        mid=tuple((np.asarray(a)+b)*.5)
        return route(a,mid,depth+1)[:-1]+route(mid,b,depth+1)
    segments=[]
    for u,v in edges:
        poly=route(tuple(points[u]),tuple(points[v]))
        segments.extend((a,b) for a,b in zip(poly,poly[1:]) if a!=b)
    splits=[{a,b} for a,b in segments]
    for i,(a,b) in enumerate(segments):
        for j in range(i+1,len(segments)):
            c,d=segments[j]
            hi=a[1]==b[1];hj=c[1]==d[1]
            if hi==hj:
                fixed=1 if hi else 0;moving=1-fixed
                if a[fixed]!=c[fixed]:continue
                for p in (a,b,c,d):
                    if min(a[moving],b[moving])<=p[moving]<=max(a[moving],b[moving]) and min(c[moving],d[moving])<=p[moving]<=max(c[moving],d[moving]):
                        splits[i].add(p);splits[j].add(p)
            else:
                p=(c[0],a[1]) if hi else (a[0],c[1])
                if all(min(x[k],y[k])<=p[k]<=max(x[k],y[k]) for x,y in ((a,b),(c,d)) for k in (0,1)):
                    splits[i].add(p);splits[j].add(p)
    result=[];ids={};result_edges=set()
    def node(p):
        if p not in ids:ids[p]=len(result);result.append(list(p))
        return ids[p]
    for (a,b),ps in zip(segments,splits):
        axis=0 if a[1]==b[1] else 1
        ps=sorted(ps,key=lambda p:p[axis])
        for c,d in zip(ps,ps[1:]):
            if c!=d:result_edges.add(tuple(sorted((node(c),node(d)))))
    return result,[list(e) for e in sorted(result_edges)],ids

def trace(rgb,page):
    h,w=rgb.shape[:2]
    black=rgb.max(2)<100
    lab,num=ndi.label(black);sizes=np.bincount(lab.ravel());sizes[0]=0
    walls=np.zeros_like(black)
    for i,sl in enumerate(ndi.find_objects(lab),1):
        if sl is not None and sizes[i]>350 and max(sl[0].stop-sl[0].start,sl[1].stop-sl[1].start)>45:
            walls|=lab==i # Keep isolated walls too; small printed glyphs are not walls.
    yy,xx=np.where(walls)
    bounds=(int(xx.min()),int(yy.min()),int(xx.max()),int(yy.max()))
    passage=~walls
    x0,y0,x1,y1=bounds
    passage[:y0+1]=False;passage[y1:]=False;passage[:,:x0+1]=False;passage[:,x1:]=False
    green=(rgb[:,:,1]>100)&(rgb[:,:,0]<rgb[:,:,1]*.6)&(rgb[:,:,2]<rgb[:,:,1]*.6)
    red=(rgb[:,:,0]>150)&(rgb[:,:,1]<rgb[:,:,0]*.5)&(rgb[:,:,2]<rgb[:,:,0]*.5)
    blue=(rgb[:,:,2]>130)&(rgb[:,:,0]<rgb[:,:,2]*.6)&(rgb[:,:,1]<rgb[:,:,2]*.9)
    start=component_centers(green,100)[0];goal=component_centers(red,100)[0]
    markers=component_centers(blue,350)[:page-25]
    # Numbering follows the labels printed on the source pages.
    if page==28: markers=sorted(markers,key=lambda p:p[1])
    elif page==29:
        markers=sorted(markers,key=lambda p:p[1],reverse=True)
        markers=[markers[0],markers[2],markers[1],markers[3]]
    else:
        markers=sorted(markers,key=lambda p:p[1],reverse=True)
        bottom,left=markers[:2];top=markers[-1];middle=sorted(markers[2:4])
        markers=[bottom,left,top,middle[0],middle[1]]
    assert len(markers)==page-25,(page,markers)
    sk=thin(passage)
    labels,count=ndi.label(passage)
    areas=np.bincount(labels.ravel());keep=areas>=400;keep[0]=False
    sk&=keep[labels]
    pixels=set(zip(*np.where(sk)))
    adj={}
    for y,x in pixels:
        ns=[]
        for dy,dx in ((-1,0),(1,0),(0,-1),(0,1),(-1,-1),(-1,1),(1,-1),(1,1)):
            q=(y+dy,x+dx)
            if q not in pixels:continue
            if dy and dx and ((y+dy,x) in pixels or (y,x+dx) in pixels):continue
            ns.append(q)
        adj[(y,x)]=ns
    anchors=[min(pixels,key=lambda q:(q[1]-p[0])**2+(q[0]-p[1])**2) for p in [start,goal]+markers]
    key={p for p,ns in adj.items() if len(ns)!=2}|set(anchors)
    points=[];ids={};edges=[];visited=set()
    def node(p):
        if p not in ids:ids[p]=len(points);points.append([float(p[1]),float(p[0])])
        return ids[p]
    for p in sorted(key):
        node(p)
        for q in adj[p]:
            if (p,q) in visited:continue
            chain=[p,q];visited.add((p,q));visited.add((q,p))
            while q not in key:
                r=next(t for t in adj[q] if t!=chain[-2])
                visited.add((q,r));visited.add((r,q));chain.append(r);q=r
            reduced=simplify(chain)
            for a,b in zip(reduced,reduced[1:]):edges.append([node(a),node(b)])
    for anchor,marker in zip(anchors,[start,goal]+markers):points[ids[anchor]]=[float(round(v)) for v in marker]
    # Every straight interpolated graph segment must stay in the white passage.
    for a,b in edges:
        p=np.asarray(points[a]);q=np.asarray(points[b]);steps=int(np.linalg.norm(q-p)*2)+1
        samples=np.rint(np.linspace(p,q,steps)).astype(int)
        assert not walls[samples[:,1],samples[:,0]].any(),(page,p,q)
    marker_coordinates=[tuple(points[ids[p]]) for p in anchors]
    points,edges,cardinal_ids=orthogonal_graph(points,edges,walls)
    marker_ids=[cardinal_ids[p] for p in marker_coordinates]
    for u,v in edges:
        a=np.asarray(points[u]);b=np.asarray(points[v])
        assert a[0]==b[0] or a[1]==b[1],('Non-cardinal edge',page,a,b)
        sample=np.rint(np.linspace(a,b,int(np.linalg.norm(b-a)*2)+2)).astype(int)
        assert not walls[sample[:,1],sample[:,0]].any(),('Wall crossing',page,a,b)
    adjacency=[[] for _ in points]
    for a,b in edges:adjacency[a].append(b);adjacency[b].append(a)
    seen={marker_ids[0]};todo=list(seen)
    for n in todo:
        for t in adjacency[n]:
            if t not in seen:seen.add(t);todo.append(t)
    region,count=ndi.label(passage)
    print('REGIONS',page,[(p,int(region[int(p[1]),int(p[0])])) for p in [start,goal]+markers])
    assert marker_ids[1] in seen,'Goal disconnected'
    unreachable=[i for i,n in enumerate(marker_ids[2:]) if n not in seen]
    seen=set(range(len(points)))
    # Exact raster wall coverage, merged vertically across equal horizontal spans.
    rects=[];active={}
    for y in range(h+1):
        row=walls[y] if y<h else np.zeros(w,bool)
        diff=np.diff(np.r_[False,row,False].astype(int));spans=list(zip(np.where(diff==1)[0],np.where(diff==-1)[0]))
        nxt={}
        for span in spans:nxt[span]=active.get(span,y)
        for (left,right),top in active.items():
            if (left,right) not in nxt:rects.append([int(left),int(top),int(right-left),int(y-top)])
        active=nxt
    original_ids=marker_ids
    # Retain enclosed passages too: the source's second maze contains an isolated leak.
    keep=sorted(seen);remap={v:i for i,v in enumerate(keep)}
    data=dict(page=page,width=w,height=h,boardWidth=10.,boardHeight=10.*h/w,bottom=(11.-10.*h/w)/2,
              points=[dict(x=points[i][0],y=points[i][1]) for i in keep],
              edges=[dict(x=remap[a],y=remap[b]) for a,b in edges if a in seen and b in seen],
              start=remap[original_ids[0]],goal=remap[original_ids[1]],
              leaks=[remap[i] for i in original_ids[2:]],
              wallRects=[dict(x=x,y=y,width=rw,height=rh) for x,y,rw,rh in rects],
              unreachableLeaks=unreachable,
              markerPixels=[dict(x=p[0],y=p[1]) for p in [start,goal]+markers])
    print(page,'nodes',len(data['points']),'edges',len(data['edges']),'wall rectangles',len(rects),'markers',data['markerPixels'])
    return data,walls

def main():
    parser=argparse.ArgumentParser();parser.add_argument('pdf');args=parser.parse_args()
    doc=fitz.open(args.pdf);OUT.mkdir(parents=True,exist_ok=True);ART.mkdir(parents=True,exist_ok=True)
    for index,page in enumerate((28,29,30),1):
        images=doc[page-1].get_images(full=True)
        xref=max(images,key=lambda t:t[2]*t[3])[0]
        raw=doc.extract_image(xref)
        rgb=np.array(Image.open(io.BytesIO(raw['image'])).convert('RGB'))
        data,walls=trace(rgb,page)
        (ART/('TEX_SapMaze_'+str(index)+'.'+raw['ext'])).write_bytes(raw['image'])
        (OUT/('MazeLayout_'+str(index)+'.json')).write_text(json.dumps(data,separators=(',',':')),encoding='utf-8')
if __name__=='__main__':main()
