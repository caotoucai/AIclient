#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "FXSignalTypeType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType fx:SIGNAL-TYPE
 *
 * Content model for the entity SIGNAL.
 */
class ASAM_FIBEX_EXPORT FXSignalType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXSignalType(FXFibex *fibex);
    ~FXSignalType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /**
     * @opt element DEFAULT-VALUE
     *
     * A signals encoded value (internal) indicating that the actual value is currently not published / not of interest
     */
    double *m_defaultValue = nullptr;

    /**
     * @required element CODING-REF
     *
     * Category of the signal
     */
    QString m_codingRef;

    FXSignalTypeType *m_sigType = nullptr;

    /**
     * @opt element PRIORITY
     *
     * A positive integer indicating the signals priority. 1 means highest priority.
     */
    quint32 *m_priority = nullptr;
};

} // FIBEX
} // ASAM

