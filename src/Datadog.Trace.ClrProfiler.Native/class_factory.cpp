// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.

#include "class_factory.h"
#include "cor_profiler.h"
#include "logging.h"
#include "version.h"

ClassFactory::ClassFactory() : refCount(0) {}

ClassFactory::~ClassFactory() = default;

HRESULT STDMETHODCALLTYPE ClassFactory::QueryInterface(REFIID riid,
                                                       void** ppvObject) {
  if (riid == IID_IUnknown || riid == IID_IClassFactory) {
    *ppvObject = this;
    this->AddRef();
    return S_OK;
  }

  *ppvObject = nullptr;
  return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE ClassFactory::AddRef() {
  return std::atomic_fetch_add(&this->refCount, 1) + 1;
}

ULONG STDMETHODCALLTYPE ClassFactory::Release() {
  const int count = std::atomic_fetch_sub(&this->refCount, 1) - 1;

  if (count <= 0) {
    delete this;
  }

  return count;
}

// profiler entry point
HRESULT STDMETHODCALLTYPE ClassFactory::CreateInstance(IUnknown* pUnkOuter,
                                                       REFIID riid,
                                                       void** ppvObject) {
  if (pUnkOuter != nullptr) {
    *ppvObject = nullptr;
    return CLASS_E_NOAGGREGATION;
  }

  trace::Info("Datadog CLR Profiler ", PROFILER_VERSION);

  profiler_ = std::make_unique<trace::CorProfiler>();
  return profiler_->QueryInterface(riid, ppvObject);
}

HRESULT STDMETHODCALLTYPE ClassFactory::LockServer(BOOL fLock) { return S_OK; }
