'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Topbar from '@/components/layout/Topbar';
import Panel from '@/components/ui/Panel';
import { api, type Article } from '@/lib/api';

const CAT_COLORS: Record<string, string> = {
  politics:     'var(--accent2)',
  finance:      'var(--accent)',
  technology:   'var(--purple)',
  sports:       'var(--yellow)',
  health:       '#22d3ee',
  entertainment:'var(--text2)',
  conflictandwar:'var(--red)',
  terrorism:    'var(--red)',
  science:      'var(--purple)',
  general:      'var(--text3)',
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

function ArticleBody({ content }: { content: string }) {
  const paragraphs = content.split(/\n{2,}/).map(p => p.trim()).filter(Boolean);
  if (paragraphs.length === 0) return (
    <p className="text-text3 text-[12px] font-mono italic">No content available.</p>
  );
  return (
    <div className="flex flex-col gap-[14px]">
      {paragraphs.map((para, i) => {
        if (/^#{1,3}\s/.test(para)) {
          const level = para.match(/^(#+)/)?.[1].length ?? 1;
          const text = para.replace(/^#+\s+/, '');
          const size = level === 1 ? 'text-[16px]' : level === 2 ? 'text-[14px]' : 'text-[13px]';
          return <p key={i} className={`font-display font-[700] ${size} text-text`}>{text}</p>;
        }
        return (
          <p key={i} className="text-[13px] text-text2 leading-[1.7]">{para}</p>
        );
      })}
    </div>
  );
}

export default function FeedDetailPage() {
  const params  = useParams<{ id: string }>();
  const router  = useRouter();
  const [article, setArticle] = useState<Article | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError]     = useState('');

  useEffect(() => {
    api.getArticle(params.id)
      .then(setArticle)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [params.id]);

  const catColor = article
    ? (CAT_COLORS[article.category.toLowerCase()] ?? 'var(--text3)')
    : 'var(--text3)';

  return (
    <>
      <Topbar title="Article detail">
        <button
          onClick={() => router.back()}
          className="flex items-center gap-[5px] px-[11px] py-[6px] rounded-btn border border-border text-text2 text-[11px] font-[500]"
        >
          <i className="ti ti-arrow-left text-[12px]" /> Back
        </button>
      </Topbar>

      <div className="flex-1 overflow-y-auto p-[18px_20px]">
        {loading && (
          <div className="text-text3 text-[12px] font-mono py-8 text-center">Loading…</div>
        )}
        {error && (
          <div className="p-[12px] rounded-[8px] text-[12px]"
            style={{ background: 'rgba(255,69,96,.1)', border: '1px solid rgba(255,69,96,.3)', color: 'var(--red)' }}>
            {error}
          </div>
        )}

        {article && (
          <div className="max-w-[780px] mx-auto flex flex-col gap-[14px]">
            {/* Header */}
            <Panel title="">
              {/* Category chip */}
              <div className="flex items-center gap-[8px] mb-[12px]">
                <span
                  className="font-mono text-[9px] uppercase tracking-[1.5px] px-[8px] py-[3px] rounded-[10px] font-[600]"
                  style={{ color: catColor, background: `${catColor}18`, border: `1px solid ${catColor}33` }}
                >
                  {article.category}
                </span>
                <span className="font-mono text-[9px] text-text3 uppercase tracking-[1px]">{article.template}</span>
              </div>

              {/* Title */}
              <h1 className="font-display text-[20px] font-[800] leading-[1.3] mb-[14px]">
                {article.title}
              </h1>

              {/* Meta row */}
              <div className="flex flex-wrap items-center gap-x-[16px] gap-y-[6px] text-[11px] text-text3 font-mono pt-[12px]"
                style={{ borderTop: '1px solid var(--border)' }}>
                {article.sourceName && (
                  <span className="flex items-center gap-[5px]">
                    <i className="ti ti-news text-[12px]" />
                    {article.sourceName}
                  </span>
                )}
                <span className="flex items-center gap-[5px]">
                  <i className="ti ti-clock text-[12px]" />
                  {formatDate(article.updatedAt)}
                </span>
                <span className="flex items-center gap-[5px]">
                  <i className="ti ti-file-text text-[12px]" />
                  {article.wordCount} words
                </span>
                <span
                  className="flex items-center gap-[5px] px-[7px] py-[2px] rounded-[6px] text-[9px] uppercase tracking-[1px] font-[600]"
                  style={
                    article.status?.toLowerCase() === 'published'
                      ? { background: 'rgba(0,229,160,.12)', color: 'var(--accent)' }
                      : { background: 'rgba(255,255,255,.06)', color: 'var(--text3)' }
                  }
                >
                  <i className={`ti ${article.status?.toLowerCase() === 'published' ? 'ti-check' : 'ti-pencil'} text-[10px]`} />
                  {article.status}
                </span>
              </div>
            </Panel>

            {/* Body */}
            <Panel title="Summary" icon="ti-article">
              {article.contentMd
                ? <ArticleBody content={article.contentMd} />
                : <p className="text-text3 text-[12px] font-mono italic">No summary available.</p>
              }
            </Panel>

            {/* Read full article */}
            {article.sourceUrl && (
              <div className="flex items-center gap-[10px] p-[14px] rounded-[10px]"
                style={{ background: 'rgba(0,136,255,.06)', border: '1px solid rgba(0,136,255,.2)' }}>
                <i className="ti ti-external-link text-[16px] flex-shrink-0" style={{ color: 'var(--accent2)' }} />
                <div className="flex-1 min-w-0">
                  <div className="text-[11px] font-[600] text-text mb-[2px]">Read the full article</div>
                  <div className="text-[10px] text-text3 font-mono truncate">{article.sourceUrl}</div>
                </div>
                <a
                  href={article.sourceUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex items-center gap-[5px] px-[12px] py-[6px] rounded-btn text-[11px] font-[600] flex-shrink-0"
                  style={{ background: 'var(--accent2)', color: '#fff' }}
                >
                  Open <i className="ti ti-arrow-up-right text-[11px]" />
                </a>
              </div>
            )}

            {/* Actions */}
            <div className="flex gap-[8px]">
              <button
                onClick={() => router.push(`/writeup?id=${article.id}`)}
                className="flex items-center gap-[6px] px-[14px] py-[8px] rounded-btn text-[12px] font-[600] text-black"
                style={{ background: 'var(--accent)' }}
              >
                <i className="ti ti-pencil text-[13px]" /> Edit in Studio
              </button>
              <button
                onClick={() => router.back()}
                className="flex items-center gap-[6px] px-[14px] py-[8px] rounded-btn border border-border text-text2 text-[12px] font-[500]"
              >
                <i className="ti ti-arrow-left text-[12px]" /> Back to feed
              </button>
            </div>
          </div>
        )}
      </div>
    </>
  );
}
